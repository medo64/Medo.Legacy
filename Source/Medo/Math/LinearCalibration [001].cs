//Copyright (c) 2012 Josip Medved <jmedved@jmedved.com>

//2012-10-30: Initial version.


using System;
using System.Collections.Generic;

namespace Medo.Math {

    /// <summary>
    /// Linear calibration using a least square regression.
    /// </summary>
    public class LinearCalibration {

        private readonly List<KeyValuePair<double, double>> CalibrationPoints = new List<KeyValuePair<double, double>>();

        /// <summary>
        /// Creates new instance.
        /// </summary>
        public LinearCalibration() { }


        /// <summary>
        /// Adds new calibration point.
        /// </summary>
        /// <param name="knownValue">Reference value.</param>
        /// <param name="measuredValue">Measured value.</param>
        public void AddCalibrationPoint(double knownValue, double measuredValue) {
            this.CalibrationPoints.Add(new KeyValuePair<double, double>(knownValue, measuredValue));
            this.Ready = false;
        }


        private double _slope;
        internal double Slope {
            get {
                if (this.Ready == false) { this.Prepare(); }
                return this._slope;
            }
            private set { this._slope = value; }
        }

        private double _intercept;
        internal double Intercept {
            get {
                if (this.Ready == false) { this.Prepare(); }
                return this._intercept;
            }
            private set { this._intercept = value; }
        }

        private double _correlationCoefficient;
        /// <summary>
        /// Gets correlation coefficient for calibration data set (R).
        /// </summary>
        public double CorrelationCoefficient {
            get {
                if (this.Ready == false) { this.Prepare(); }
                return this._correlationCoefficient;
            }
            private set { this._correlationCoefficient = value; }
        }

        private double _coefficientOfDetermination;
        /// <summary>
        /// Gets coefficient of determination (R^2) that indicates how well regression line fits calibration points.
        /// </summary>
        public double CoefficientOfDetermination {
            get {
                if (this.Ready == false) { this.Prepare(); }
                return this._coefficientOfDetermination;
            }
            private set { this._coefficientOfDetermination = value; }
        }


        private bool Ready = false;

        private void Prepare() {
            if (this.CalibrationPoints.Count == 0) { //no calibration
                this.Slope = 1;
                this.Intercept = 0;
                this.CorrelationCoefficient = 1;
                this.CoefficientOfDetermination = 1;
            } else if (this.CalibrationPoints.Count == 1) { //no calibration - just offset
                this.Slope = 1;
                this.Intercept = this.CalibrationPoints[0].Value - this.CalibrationPoints[0].Key;
                this.CorrelationCoefficient = 1;
                this.CoefficientOfDetermination = 1;
            } else {

                double n = this.CalibrationPoints.Count;
                double sumX = 0;
                double sumY = 0;
                double sumX2 = 0;
                double sumY2 = 0;
                double sumXY = 0;
                foreach (var point in this.CalibrationPoints) {
                    var x = point.Key;
                    var y = point.Value;
                    sumX += x;
                    sumY += y;
                    sumX2 += x * x;
                    sumY2 += y * y;
                    sumXY += x * y;
                }

                var mT = (n * sumXY - sumX * sumY);
                var mB = (n * sumX2 - sumX * sumX);
                var m = mT / mB;
                var r = mT / System.Math.Sqrt(mB * (n * sumY2 - sumY * sumY));

                this.Slope = m;
                this.Intercept = (sumY / n) - m * (sumX / n); ;
                this.CorrelationCoefficient = r;
                this.CoefficientOfDetermination = r * r;
            }

            this.Ready = true;
        }


        /// <summary>
        /// Returns value adjusted using a least square regression.
        /// </summary>
        /// <param name="value">Value to adjust.</param>
        public double GetAdjustedValue(double value) {
            return (value - this.Intercept) / this.Slope;
        }

    }

}
