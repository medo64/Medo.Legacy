using System;
using Medo.Device;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test {

    [TestClass()]
    public class SerialGpsTests {

        public TestContext TestContext { get; set; }


        [TestMethod()]
        public void SerialGps_Position_Basic() {
            var x = new GpsPosition(45.5575, 18.6796, 87);

            Assert.AreEqual(45.5575, x.Latitude);
            Assert.AreEqual(18.6796, x.Longitude);
            Assert.AreEqual(87, x.Altitude);

            Assert.IsTrue(x.HasLongitude);
            Assert.IsTrue(x.HasLatitude);
            Assert.IsTrue(x.HasAltitude);

            Assert.AreEqual("45.5575° N, 18.6796° E", x.ToString());
        }

        [TestMethod()]
        public void SerialGps_Position_Empty() {
            var x = new GpsPosition();

            Assert.AreEqual(double.NaN, x.Latitude);
            Assert.AreEqual(double.NaN, x.Longitude);
            Assert.AreEqual(double.NaN, x.Altitude);

            Assert.IsFalse(x.HasLongitude);
            Assert.IsFalse(x.HasLatitude);
            Assert.IsFalse(x.HasAltitude);
        }

        [TestMethod()]
        public void SerialGps_Position_Equals() {
            var x1 = new GpsPosition(45.5575, 18.6796, 87);
            var x2 = new GpsPosition(45.5575, 18.6796, 87);
            var x3 = new GpsPosition(45.5575, 18.6796, double.NaN);

            Assert.IsTrue(x1.Equals(x2));
            Assert.IsTrue(x2.Equals(x1));
            Assert.IsFalse(x1.Equals(x3));
        }

        [TestMethod()]
        public void SerialGps_Position_Distance() {
            var x1 = new GpsPosition(45.5575, 18.6796, 87);
            var x2 = new GpsPosition(45.6000, 18.4667, 88);

            Assert.AreEqual(17230, Math.Round(GpsPosition.DistanceBetween(x1, x2)));
            Assert.AreEqual(GpsPosition.DistanceBetween(x1, x2), GpsPosition.DistanceBetween(x2, x1));
            Assert.AreEqual(GpsPosition.DistanceBetween(x1, x2), x1.DistanceTo(x2));
            Assert.AreEqual(GpsPosition.DistanceBetween(x1, x2), x2.DistanceTo(x1));
        }


        [TestMethod()]
        public void SerialGps_Velocity_Basic() {
            var x = new GpsVelocity(10, 45, 1);

            Assert.AreEqual(10, x.Speed);
            Assert.AreEqual(36, x.SpeedInKph);
            Assert.AreEqual(22.3694, Math.Round(x.SpeedInMph, 4));
            Assert.AreEqual(19.4384, Math.Round(x.SpeedInKnots, 4));
            Assert.AreEqual(32.8084, Math.Round(x.SpeedInFtps, 4));
            Assert.AreEqual(45, x.Heading);
            Assert.AreEqual(1, x.MagneticVariation);

            Assert.IsTrue(x.HasSpeed);
            Assert.IsTrue(x.HasHeading);
            Assert.IsTrue(x.HasMagneticVariation);

            Assert.AreEqual("10.0 m/s", x.ToString());
        }

        [TestMethod()]
        public void SerialGps_Velocity_Empty() {
            var x = new GpsVelocity();

            Assert.AreEqual(double.NaN, x.Speed);
            Assert.AreEqual(double.NaN, x.Heading);
            Assert.AreEqual(double.NaN, x.MagneticVariation);

            Assert.IsFalse(x.HasSpeed);
            Assert.IsFalse(x.HasHeading);
            Assert.IsFalse(x.HasMagneticVariation);
        }

        [TestMethod()]
        public void SerialGps_Velocity_Equals() {
            var x1 = new GpsVelocity(21, 0, 0);
            var x2 = new GpsVelocity(21, 0, 0);
            var x3 = new GpsVelocity(21, 0, double.NaN);

            Assert.IsTrue(x1.Equals(x2));
            Assert.IsTrue(x2.Equals(x1));
            Assert.IsFalse(x1.Equals(x3));
        }


        [TestMethod()]
        public void SerialGps_Geometry_Basic() {
            var x = new GpsGeometry(1.2, 2.3, 3.4, 5, 6);

            Assert.AreEqual(1.2, x.HorizontalDilution);
            Assert.AreEqual(2.3, x.VerticalDilution);
            Assert.AreEqual(3.4, x.PositionDilution);
            Assert.AreEqual(5, x.SatellitesInUse);
            Assert.AreEqual(6, x.SatellitesInView);

            Assert.IsTrue(x.HasHorizontalDilution);
            Assert.IsTrue(x.HasVerticalDilution);
            Assert.IsTrue(x.HasPositionDilution);
            Assert.IsTrue(x.HasAnySatellitesInUse);
            Assert.IsTrue(x.HasAnySatellitesInView);

            Assert.AreEqual("3.4", x.ToString());
        }

        [TestMethod()]
        public void SerialGps_Geometry_Empty() {
            var x = new GpsGeometry();

            Assert.AreEqual(double.NaN, x.HorizontalDilution);
            Assert.AreEqual(double.NaN, x.VerticalDilution);
            Assert.AreEqual(double.NaN, x.PositionDilution);
            Assert.AreEqual(0, x.SatellitesInUse);
            Assert.AreEqual(0, x.SatellitesInView);

            Assert.IsFalse(x.HasHorizontalDilution);
            Assert.IsFalse(x.HasVerticalDilution);
            Assert.IsFalse(x.HasPositionDilution);
            Assert.IsFalse(x.HasAnySatellitesInUse);
            Assert.IsFalse(x.HasAnySatellitesInView);

            Assert.AreEqual("", x.ToString());
        }

        [TestMethod()]
        public void SerialGps_Geometry_Equals() {
            var x1 = new GpsGeometry(1.2, 2.3, 3.4, 5, 6);
            var x2 = new GpsGeometry(1.2, 2.3, 3.4, 5, 6);
            var x3 = new GpsGeometry(1.2, 2.3, 3.4);

            Assert.IsTrue(x1.Equals(x2));
            Assert.IsTrue(x2.Equals(x1));
            Assert.IsFalse(x1.Equals(x3));
        }


        [TestMethod()]
        public void SerialGps_Sentence_Rmc() {
            var x = new PrivateType(typeof(SerialGps));
            var r = x.InvokeStatic("ParseLine", "$GPRMC,212946.00,A,4533.14884,N,01843.80437,E,6.095,281.36,090315,,,A*68");

            Assert.AreEqual(new DateTime(2015, 03, 09, 21, 29, 46, DateTimeKind.Utc), (DateTime)GetFieldValue(r, "Time"));
            Assert.AreEqual(45.552481, Math.Round((double)GetFieldValue(r, "Latitude"), 6));
            Assert.AreEqual(18.730073, Math.Round((double)GetFieldValue(r, "Longitude"), 6));
            Assert.AreEqual(3.135539, Math.Round((double)GetFieldValue(r, "Speed"), 6));
            Assert.AreEqual(281.36, Math.Round((double)GetFieldValue(r, "Heading"), 6));
        }

        [TestMethod()]
        public void SerialGps_Sentence_Gga() {
            var x = new PrivateType(typeof(SerialGps));
            var r = x.InvokeStatic("ParseLine", "$GPGGA,212946.00,4533.14884,N,01843.80437,E,1,06,1.43,89.9,M,38.6,M,,*65");

            Assert.AreEqual(45.552481, Math.Round((double)GetFieldValue(r, "Latitude"), 6));
            Assert.AreEqual(18.730073, Math.Round((double)GetFieldValue(r, "Longitude"), 6));
            Assert.AreEqual(89.9, Math.Round((double)GetFieldValue(r, "Altitude"), 6));
            Assert.AreEqual(6, (int)GetFieldValue(r, "SatellitesInUse"));
        }

        [TestMethod()]
        public void SerialGps_Sentence_Gll() {
            var x = new PrivateType(typeof(SerialGps));
            var r = x.InvokeStatic("ParseLine", "$GPGLL,4533.14884,N,01843.80437,E,212946.00,A,A*65");

            Assert.AreEqual(45.552481, Math.Round((double)GetFieldValue(r, "Latitude"), 6));
            Assert.AreEqual(18.730073, Math.Round((double)GetFieldValue(r, "Longitude"), 6));
        }

        [TestMethod()]
        public void SerialGps_Sentence_Gsa() {
            var x = new PrivateType(typeof(SerialGps));
            var r = x.InvokeStatic("ParseLine", "$GPGSA,A,3,12,29,25,02,05,31,,,,,,,2.38,1.43,1.90*0F");

            Assert.AreEqual(2.38, Math.Round((double)GetFieldValue(r, "HorizontalDilution"), 6));
            Assert.AreEqual(1.43, Math.Round((double)GetFieldValue(r, "VerticalDilution"), 6));
            Assert.AreEqual(1.90, Math.Round((double)GetFieldValue(r, "PositionDilution"), 6));
        }

        [TestMethod()]
        public void SerialGps_Sentence_Gsv_01() {
            var x = new PrivateType(typeof(SerialGps));
            var r = x.InvokeStatic("ParseLine", "$GPGSV,3,1,09,02,14,044,20,05,22,077,27,08,38,303,39,10,01,023,*7F");

            Assert.AreEqual(9, (int)GetFieldValue(r, "SatellitesInView"));
        }

        [TestMethod()]
        public void SerialGps_Sentence_Gsv_02() {
            var x = new PrivateType(typeof(SerialGps));
            var r = x.InvokeStatic("ParseLine", "$GPGSV,3,2,09,12,16,122,27,21,,,30,25,54,124,26,29,70,042,32*4F");

            Assert.AreEqual(9, (int)GetFieldValue(r, "SatellitesInView"));
        }

        [TestMethod()]
        public void SerialGps_Sentence_Gsv_03() {
            var x = new PrivateType(typeof(SerialGps));
            var r = x.InvokeStatic("ParseLine", "$GPGSV,3,3,09,31,49,262,40*4D");

            Assert.AreEqual(9, (int)GetFieldValue(r, "SatellitesInView"));
        }

        [TestMethod()]
        public void SerialGps_Sentence_Vtg() {
            var x = new PrivateType(typeof(SerialGps));
            var r = x.InvokeStatic("ParseLine", "$GPVTG,281.36,T,,M,6.095,N,11.288,K,A*0B");

            Assert.AreEqual(3.135539, Math.Round((double)GetFieldValue(r, "Speed"), 6));
            Assert.AreEqual(281.36, Math.Round((double)GetFieldValue(r, "Heading"), 6));
        }


        #region Helper

        private static object GetFieldValue(object readings, string name) {
            var fieldInfo = readings.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return fieldInfo.GetValue(readings);
        }

        #endregion

    }
}
