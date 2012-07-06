﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Nancy;
using DinnerParty.Models;
using Nancy.RouteHelpers;
using DinnerParty.Models.RavenDB;
using Raven.Client.Linq;

namespace DinnerParty.Modules
{


    public class SearchModule : PersistModule
    {
        public SearchModule()
            : base("/search")
        {
            Post["/GetMostPopularDinners"] = parameters =>
                {

                    // Default the limit to 40, if not supplied.
                    if (!this.Request.Form.limit.HasValue || String.IsNullOrWhiteSpace(this.Request.Form.limit))
                        this.Request.Form.limit = 40;

                    var jsonDinners = DocumentSession.Query<JsonDinner, Dinners_Index>()
                        .Where(x => x.EventDate >= DateTime.Now.Date)
                        .Take((int)this.Request.Form.limit)
                        .OrderByDescending(x => x.RSVPCount)
                        .AsProjection<JsonDinner>()
                        .ToList();

                    return Response.AsJson(jsonDinners);
                };

            Post["/SearchByLocation"] = parameters =>
            {

                double latitude = (double)this.Request.Form.latitude;
                double longitude = (double)this.Request.Form.longitude;

                var dinners = DocumentSession.Query<Dinner, Dinners_Index>()
                                .Where(x => x.EventDate > DateTime.Now.Date)
                                .AsEnumerable()
                                .Where(x => DistanceBetween(x.Latitude, x.Longitude, latitude, longitude) < 1000).Select(x => JsonDinnerFromDinner(x));

                return Response.AsJson(dinners.ToList());
            };
        }

        /// <summary>
        /// C# Replacement for Stored Procedure
        /// </summary>
        /// <param name="Latitude"></param>
        /// <param name="Longitude"></param>
        /// <remarks>
        /// CREATE FUNCTION [dbo].[DistanceBetween] (@Lat1 as real,
        ///                @Long1 as real, @Lat2 as real, @Long2 as real)
        ///RETURNS real
        ///AS
        ///BEGIN
        ///
        ///DECLARE @dLat1InRad as float(53);
        ///SET @dLat1InRad = @Lat1 * (PI()/180.0);
        ///DECLARE @dLong1InRad as float(53);
        ///SET @dLong1InRad = @Long1 * (PI()/180.0);
        ///DECLARE @dLat2InRad as float(53);
        ///SET @dLat2InRad = @Lat2 * (PI()/180.0);
        ///DECLARE @dLong2InRad as float(53);
        ///SET @dLong2InRad = @Long2 * (PI()/180.0);
        ///
        ///DECLARE @dLongitude as float(53);
        ///SET @dLongitude = @dLong2InRad - @dLong1InRad;
        ///DECLARE @dLatitude as float(53);
        ///SET @dLatitude = @dLat2InRad - @dLat1InRad;
        ///* Intermediate result a. */
        ///DECLARE @a as float(53);
        ///SET @a = SQUARE (SIN (@dLatitude / 2.0)) + COS (@dLat1InRad)
        ///* COS (@dLat2InRad)
        ///* SQUARE(SIN (@dLongitude / 2.0));
        ///* Intermediate result c (great circle distance in Radians). */
        ///DECLARE @c as real;
        ///SET @c = 2.0 * ATN2 (SQRT (@a), SQRT (1.0 - @a));
        ///DECLARE @kEarthRadius as real;
        ///* SET kEarthRadius = 3956.0 miles */
        ///SET @kEarthRadius = 6376.5;        /* kms */
        ///
        ///DECLARE @dDistance as real;
        ///SET @dDistance = @kEarthRadius * @c;
        ///return (@dDistance);
        ///END
        /// </remarks>
        /// <returns></returns>
        private double DistanceBetween(double Lat1, double Long1, double Lat2, double Long2)
        {
            double dLat1InRad = Lat1 * (Math.PI / 180.0);
            double dLong1InRad = Long1 * (Math.PI / 180.0);
            double dLat2InRad = Lat2 * (Math.PI / 180.0);
            double dLong2InRad = Long2 * (Math.PI / 180.0);

            double dLongitude = dLong2InRad - dLong1InRad;
            double dLatitude = dLat2InRad - dLat1InRad;
            ///* Intermediate result a. */
            double a = Math.Pow(Math.Sin(dLatitude / 2.0), 2) + Math.Cos(dLat1InRad)
                             * Math.Cos(dLat2InRad)
                             * Math.Pow(Math.Sin(dLongitude / 2.0), 2);
            ///* Intermediate result c (great circle distance in Radians). */
            double c = 2.0 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1.0 - a));
            double kEarthRadius = 6376.5;        /* kms */

            double dDistance = kEarthRadius * c;
            return dDistance;
        }

        private JsonDinner JsonDinnerFromDinner(Dinner dinner)
        {
            return new JsonDinner
            {
                DinnerID = dinner.DinnerID,
                EventDate = dinner.EventDate,
                Latitude = dinner.Latitude,
                Longitude = dinner.Longitude,
                Title = dinner.Title,
                Description = dinner.Description,
                RSVPCount = dinner.RSVPs.Count,

                //TODO: Need to mock this out for testing...
                //Url = Url.RouteUrl("PrettyDetails", new { Id = dinner.DinnerID } )
                Url = dinner.DinnerID.ToString()
            };
        }
    }
}