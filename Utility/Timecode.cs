using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Flow.Utility
{
    /// <summary>
    /// Represents a SMPTE 12M standard time code and provides conversion operations to various SMPTE time code formats and rates.
    /// </summary>
    /// <remarks>
    /// Framerates supported by the TimeCode class include, 23.98 IVTC Film Sync, 24fps Film Sync, 25fps PAL, 29.97 drop frame,
    /// 29.97 Non drop, and 30fps.
    /// </remarks>
    public struct TimeCode : IComparable, IComparable<TimeCode>, IEquatable<TimeCode>
    {
        #region Private Fields

        /// <summary>
        /// Regular expression string used for parsing out the timecode.
        /// </summary>
        private const string SmpteRegExString = "(?<Hours>\\d{2}):(?<Minutes>\\d{2}):(?<Seconds>\\d{2})(?::|;)(?<Frames>\\d{2})";

        /// <summary>
        /// Regular expression object used for validating timecode.
        /// </summary>
        private static readonly Regex ValidateTimecode = new Regex(SmpteRegExString, RegexOptions.CultureInvariant);

        /// <summary>
        /// The private Timespan used to track absolute time for this instance.
        /// </summary>
        private readonly double _absoluteTime;

        /// <summary>
        /// The frame rate for this instance.
        /// </summary>
        private readonly SmpteFrameRate _frameRate;

        #endregion

        #region Constructors

        /// <summary>
        ///  Initializes a new instance of the TimeCode struct to a specified number of hours, minutes, and seconds.
        /// </summary>
        /// <param name="hours">Number of hours.</param>
        /// <param name="minutes">Number of minutes.</param>
        /// <param name="seconds">Number of seconds.</param>
        /// <param name="frames">Number of frames.</param>
        /// <param name="rate">The SMPTE frame rate.</param>
        /// <exception cref="System.FormatException">
        /// The parameters specify a TimeCode value less than TimeCode.MinValue.
        /// or greater than TimeCode.MaxValue, or the values of time code components are not valid for the SMPTE framerate.
        /// </exception>
        /// <code source="..\Documentation\SdkDocSamples\TimecodeSamples.cs" region="CreateTimeCode_2398FromIntegers" lang="CSharp" title="Create TimeCode from Integers"/>
        public TimeCode(int hours, int minutes, int seconds, int frames, SmpteFrameRate rate)
        {
            var timeCode = String.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}:{2:D2}:{3:D2}", hours, minutes, seconds, frames);
            _frameRate = rate;
            _absoluteTime = Smpte12MToAbsoluteTime(timeCode, _frameRate);
        }

        /// <summary>
        /// Initializes a new instance of the TimeCode struct using a time code string and a SMPTE framerate.
        /// </summary>
        /// <param name="timeCode">The SMPTE 12m time code string.</param>
        /// <param name="rate">The SMPTE framerate used for this instance of TimeCode.</param>
        public TimeCode(string timeCode, SmpteFrameRate rate)
        {
            _frameRate = rate;
            _absoluteTime = Smpte12MToAbsoluteTime(timeCode, _frameRate);
        }

        /// <summary>
        /// Initializes a new instance of the TimeCode struct using an absolute time value, and the SMPTE framerate.
        /// </summary>
        /// <param name="absoluteTime">The double that represents the absolute time value.</param>
        /// <param name="rate">The SMPTE framerate that this instance should use.</param>
        public TimeCode(double absoluteTime, SmpteFrameRate rate)
        {
            _absoluteTime = absoluteTime;
            _frameRate = rate;
        }


        #endregion

        #region Public Static Properties

        /// <summary>
        ///  Gets the number of ticks in 1 day. 
        ///  This field is constant.
        /// </summary>
        public static long TicksPerDay
        {
            get { return 864000000000; }
        }

        /// <summary>
        ///  Gets the number of absolute time ticks in 1 day. 
        ///  This field is constant.
        /// </summary>
        public static double TicksPerDayAbsoluteTime
        {
            get { return 86400; }
        }

        /// <summary>
        ///  Gets the number of ticks in 1 hour. This field is constant.
        /// </summary>
        public static long TicksPerHour
        {
            get { return 36000000000; }
        }

        /// <summary>
        ///  Gets the number of absolute time ticks in 1 hour. This field is constant.
        /// </summary>
        public static double TicksPerHourAbsoluteTime
        {
            get { return 3600; }
        }

        /// <summary>
        /// Gets the number of ticks in 1 millisecond. This field is constant.
        /// </summary>
        public static long TicksPerMillisecond
        {
            get { return 10000; }
        }

        /// <summary>
        /// Gets the number of ticks in 1 millisecond. This field is constant.
        /// </summary>
        public static double TicksPerMillisecondAbsoluteTime
        {
            get { return 0.0010000D; }
        }

        /// <summary>
        /// Gets the number of ticks in 1 minute. This field is constant.
        /// </summary>
        public static long TicksPerMinute
        {
            get { return 600000000; }
        }

        /// <summary>
        /// Gets the number of absolute time ticks in 1 minute. This field is constant.
        /// </summary>
        public static double TicksPerMinuteAbsoluteTime
        {
            get { return 60; }
        }

        /// <summary>
        /// Gets the number of ticks in 1 second.
        /// </summary>
        public static long TicksPerSecond
        {
            get { return 10000000; }
        }

        /// <summary>
        /// Gets the number of ticks in 1 second.
        /// </summary>
        public static double TicksPerSecondAbsoluteTime
        {
            get { return 1.0000000D; }
        }

        /// <summary>
        ///  Gets the maximum TimeCode value. The Max value for Timecode. This field is read-only.
        /// </summary>
        public double MaxValue
        {
            get
            {
                switch (_frameRate)
                {
                    case SmpteFrameRate.Smpte2398:
                        return 86486.3582916667;
                    case SmpteFrameRate.Smpte24:
                        return 86399.9583333333;
                    case SmpteFrameRate.Smpte25:
                        return 86399.9600000000;
                    case SmpteFrameRate.Smpte2997Drop:
                        return 86399.8802333333;
                    case SmpteFrameRate.Smpte2997NonDrop:
                        return 86486.3666333333;
                    case SmpteFrameRate.Smpte30:
                        return 86399.9666666667;
                    default:
                        return 86424;
                }
            }
        }

        /// <summary>
        /// Gets the minimum TimeCode value. This field is read-only.
        /// </summary>
        public static double MinValue
        {
            get { return 0; }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the absolute time in seconds of the current TimeCode object.
        /// </summary>
        /// <returns>
        ///  A double that is the absolute time in seconds duration of the current TimeCode object.
        /// </returns>
        public double Duration
        {
            get { return _absoluteTime; }
        }

        /// <summary>
        /// Gets or the current SMPTE framerate for this TimeCode instance.
        /// </summary>
        public SmpteFrameRate FrameRate
        {
            get { return _frameRate; }
        }

        /// <summary>
        ///  Gets the number of whole hours represented by the current TimeCode
        ///  structure.
        /// </summary>
        /// <returns>
        ///  The hour component of the current TimeCode structure. The return value
        ///     ranges from 0 through 23.
        /// </returns>
        public int HoursSegment
        {
            get
            {
                var timeCode = AbsoluteTimeToSmpte12M(_absoluteTime, _frameRate);
                var hours = timeCode.Substring(0, 2);
                return Int32.Parse(hours, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the number of whole minutes represented by the current TimeCode structure.
        /// </summary>
        /// <returns>
        /// The minute component of the current TimeCode structure. The return
        /// value ranges from 0 through 59.
        /// </returns>
        public int MinutesSegment
        {
            get
            {
                var timeCode = AbsoluteTimeToSmpte12M(_absoluteTime, _frameRate);
                var minutes = timeCode.Substring(3, 2);
                return Int32.Parse(minutes, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the number of whole seconds represented by the current TimeCode structure.
        /// </summary>
        /// <returns>
        ///  The second component of the current TimeCode structure. The return
        ///    value ranges from 0 through 59.
        /// </returns>
        public int SecondsSegment
        {
            get
            {
                var timeCode = AbsoluteTimeToSmpte12M(_absoluteTime, _frameRate);
                var seconds = timeCode.Substring(6, 2);
                return Int32.Parse(seconds, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the number of whole frames represented by the current TimeCode
        ///     structure.
        /// </summary>
        /// <returns>
        /// The frame component of the current TimeCode structure. The return
        ///     value depends on the framerate selected for this instance. All frame counts start at zero.
        /// </returns>
        public int FramesSegment
        {
            get
            {
                var timeCode = AbsoluteTimeToSmpte12M(_absoluteTime, _frameRate);
                var frames = timeCode.Substring(9, 2);
                return Int32.Parse(frames, CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Gets the value of the current TimeCode structure expressed in whole
        ///     and fractional hours.
        /// </summary>
        /// <returns>
        ///  The total number of hours represented by this instance.
        /// </returns>
        public double TotalHours
        {
            get
            {
                var framecount = AbsoluteTimeToFrames(_absoluteTime, _frameRate);
                return (framecount / 108000D) % 24;
            }
        }

        /// <summary>
        /// Gets the value of the current TimeCode structure expressed in whole
        /// and fractional minutes.
        /// </summary>
        /// <returns>
        ///  The total number of minutes represented by this instance.
        /// </returns>
        public double TotalMinutes
        {
            get
            {
                var framecount = AbsoluteTimeToFrames(_absoluteTime, _frameRate);

                double minutes;

                switch (_frameRate)
                {
                    case SmpteFrameRate.Smpte2398:
                    case SmpteFrameRate.Smpte24:
                        minutes = framecount / 1400D;
                        break;
                    case SmpteFrameRate.Smpte25:
                        minutes = framecount / 1500D;
                        break;
                    case SmpteFrameRate.Smpte2997Drop:
                    case SmpteFrameRate.Smpte2997NonDrop:
                    case SmpteFrameRate.Smpte30:
                        minutes = framecount / 1800D;
                        break;
                    default:
                        minutes = framecount / 1800D;
                        break;
                }

                return minutes;
            }
        }

        /// <summary>
        /// Gets the value of the current TimeCode structure expressed in whole
        /// and fractional seconds.
        /// </summary>
        /// <returns>
        /// The total number of seconds represented by this instance.
        /// </returns>
        public double TotalSeconds
        {
            get
            {
                return _absoluteTime;
            }
        }

        /// <summary>
        /// Gets the value of the current TimeCode structure expressed in frames.
        /// </summary>
        /// <returns>
        ///  The total number of frames represented by this instance.
        /// </returns>
        public long TotalFrames
        {
            get
            {
                return AbsoluteTimeToFrames(_absoluteTime, _frameRate);
            }
        }

        #endregion

        #region Operator Overloads

        /// <summary>
        /// Subtracts a specified TimeCode from another specified TimeCode.
        /// </summary>
        /// <param name="time1">The first TimeCode.</param>
        /// <param name="time2">The second TimeCode.</param>
        /// <returns>A TimeCode whose value is the result of the value of time1 minus the value of time2.
        /// </returns>
        /// <exception cref="System.OverflowException">The return value is less than TimeCode.MinValue or greater than TimeCode.MaxValue.
        /// </exception>
        public static TimeCode operator -(TimeCode time1, TimeCode time2)
        {
            var time = time1._absoluteTime - time2._absoluteTime;

            if (time < MinValue)
            {
                throw new OverflowException("Smpte12MOverflowException");
            }

            return new TimeCode(time, time1.FrameRate);
        }

        /// <summary>
        /// Adds two specified TimeCode instances.
        /// </summary>
        /// <param name="time1">The first TimeCode.</param>
        /// <param name="time2">The second TimeCode.</param>
        /// <returns>A TimeCode whose value is the sum of the values of time1 and time2.</returns>
        /// <exception cref="System.OverflowException">
        /// The resulting TimeCode is less than TimeCode.MinValue or greater than TimeCode.MaxValue.
        /// </exception>
        public static TimeCode operator +(TimeCode time1, TimeCode time2)
        {
            var time3 = new TimeCode(time1._absoluteTime + time2._absoluteTime, time1.FrameRate);
            if (time3._absoluteTime >= time1.MaxValue)
            {
                throw new OverflowException("Smpte12MOverflowException");
            }

            return time3;
        }

        /// <summary>
        ///  Indicates whether a specified TimeCode is less than another
        ///  specified TimeCode.
        /// </summary>
        /// <param name="time1">The first TimeCode.</param>
        /// <param name="time2">The second TimeCode.</param>
        /// <returns> True if the value of time1 is less than the value of time2; otherwise, false.</returns>
        public static bool operator <(TimeCode time1, TimeCode time2)
        {
            return time1._absoluteTime < time2._absoluteTime;
        }

        /// <summary>
        /// Indicates whether a specified TimeCode is greater than another specified
        ///     TimeCode.
        /// </summary>
        /// <param name="time1">The first TimeCode.</param>
        /// <param name="time2">The second TimeCode.</param>
        /// <returns>true if the value of time1 is greater than the value of time2; otherwise, false.
        /// </returns>
        public static bool operator >(TimeCode time1, TimeCode time2)
        {
            return time1._absoluteTime > time2._absoluteTime;
        }

        /// <summary>
        ///  Indicates whether two TimeCode instances are equal.
        /// </summary>
        /// <param name="time1">The first TimeCode.</param>
        /// <param name="time2">The second TimeCode.</param>
        /// <returns>true if the values of time1 and time2 are equal; otherwise, false.</returns>
        public static bool operator ==(TimeCode time1, TimeCode time2)
        {
            var diff = Math.Abs(time1._absoluteTime - time2._absoluteTime);

            return diff < 0.0001; // within a millisecond is close enough          
        }

        /// <summary>
        /// Indicates whether two TimeCode instances are not equal.
        /// </summary>
        /// <param name="time1">The first TimeCode.</param>
        /// <param name="time2">The second TimeCode.</param>
        /// <returns>true if the values of time1 and time2 are not equal; otherwise, false.</returns>
        public static bool operator !=(TimeCode time1, TimeCode time2)
        {
            return !(time1 == time2);
        }

        /// <summary>
        ///  Indicates whether a specified TimeCode is less than or equal to another
        ///  specified TimeCode.
        /// </summary>
        /// <param name="time1">The first TimeCode.</param>
        /// <param name="time2">The second TimeCode.</param>
        /// <returns>True if the value of time1 is less than or equal to the value of time2; otherwise, false.</returns>
        public static bool operator <=(TimeCode time1, TimeCode time2)
        {
            if ((time1._absoluteTime < time2._absoluteTime) || (time1 == time2))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Indicates whether a specified TimeCode is greater than or equal to
        ///     another specified TimeCode.
        /// </summary>
        /// <param name="time1">The first TimeCode.</param>
        /// <param name="time2">The second TimeCode.</param>
        /// <returns>True if the value of time1 is greater than or equal to the value of time2; otherwise, false.</returns>
        public static bool operator >=(TimeCode time1, TimeCode time2)
        {
            if ((time1._absoluteTime > time2._absoluteTime) || (time1 == time2))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Compares two TimeCode values and returns an integer that indicates their relationship.
        /// </summary>
        /// <param name="time1">The first TimeCode.</param>
        /// <param name="time2">The second TimeCode.</param>
        /// <returns>
        /// Value Condition -1 time1 is less than time2, 0 time1 is equal to time2, 1 time1 is greater than time2.
        /// </returns>
        public static int Compare(TimeCode time1, TimeCode time2)
        {
            if (time1 < time2)
            {
                return -1;
            }

            if (time1 == time2)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        ///  Returns a value indicating whether two specified instances of TimeCode
        ///  are equal.
        /// </summary>
        /// <param name="time1">The first TimeCode.</param>
        /// <param name="time2">The second TimeCode.</param>
        /// <returns>true if the values of time1 and time2 are equal; otherwise, false.</returns>
        public static bool Equals(TimeCode time1, TimeCode time2)
        {
            if (time1 == time2)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Public Static Methods

        public static SmpteFrameRate FramerateFromTimecodeMode(int mode)
        {
            switch(mode)
            {
                case 25:
                    return SmpteFrameRate.Smpte25;

                case 30:
                    return SmpteFrameRate.Smpte2997NonDrop;

                case 286:
                    return SmpteFrameRate.Smpte2997Drop;

                default:
                    throw new Exception("Invalid timecode mode. Should be 25, 30 or 286.");
            }
        }


        /// <summary>
        ///  Returns a TimeCode that represents a specified number of hours, where
        ///  the specification is accurate to the nearest millisecond.
        /// </summary>
        /// <param name="hours">A number of hours accurate to the nearest millisecond.</param>
        /// <param name="rate">The desired framerate for this instance.</param>
        /// <returns> A TimeCode that represents value.</returns>
        /// <exception cref="System.OverflowException">
        /// value is less than TimeCode.MinValue or greater than TimeCode.MaxValue.
        /// -or-value is System.Double.PositiveInfinity.-or-value is System.Double.NegativeInfinity.
        /// </exception>
        /// <exception cref="System.FormatException">
        /// value is equal to System.Double.NaN.
        /// </exception>
        public static TimeCode FromHours(double hours, SmpteFrameRate rate)
        {
            var absoluteTime = hours * TicksPerHourAbsoluteTime;
            return new TimeCode(absoluteTime, rate);
        }

        /// <summary>
        ///   Returns a TimeCode that represents a specified number of minutes,
        ///   where the specification is accurate to the nearest millisecond.
        /// </summary>
        /// <param name="minutes">A number of minutes, accurate to the nearest millisecond.</param>
        /// <param name="rate">The <see cref="SmpteFrameRate"/> to use for the calculation.</param>
        /// <returns>A TimeCode that represents value.</returns>
        /// <exception cref="System.OverflowException">
        /// value is less than TimeCode.MinValue or greater than TimeCode.MaxValue.-or-value
        /// is System.Double.PositiveInfinity.-or-value is System.Double.NegativeInfinity.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// value is equal to System.Double.NaN.
        /// </exception>
        public static TimeCode FromMinutes(double minutes, SmpteFrameRate rate)
        {
            var absoluteTime = minutes * TicksPerMinuteAbsoluteTime;
            return new TimeCode(absoluteTime, rate);
        }

        /// <summary>
        /// Returns a TimeCode that represents a specified number of seconds,
        /// where the specification is accurate to the nearest millisecond.
        /// </summary>
        /// <param name="seconds">A number of seconds, accurate to the nearest millisecond.</param>
        /// /// <param name="rate">The framerate of the Timecode.</param>
        /// <returns>A TimeCode that represents value.</returns>
        /// <exception cref="System.OverflowException">
        /// value is less than TimeCode.MinValue or greater than TimeCode.MaxValue.-or-value
        ///  is System.Double.PositiveInfinity.-or-value is System.Double.NegativeInfinity.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// value is equal to System.Double.NaN.
        /// </exception>
        public static TimeCode FromSeconds(double seconds, SmpteFrameRate rate)
        {
            return new TimeCode(seconds, rate);
        }

        /// <summary>
        /// Returns a TimeCode that represents a specified number of frames.
        /// </summary>
        /// <param name="frames">A number of frames.</param>
        /// <param name="rate">The framerate of the Timecode.</param>
        /// <returns>A TimeCode that represents value.</returns>
        /// <exception cref="System.OverflowException">
        ///  value is less than TimeCode.MinValue or greater than TimeCode.MaxValue.-or-value
        ///    is System.Double.PositiveInfinity.-or-value is System.Double.NegativeInfinity.
        /// </exception>
        /// <exception cref="System.ArgumentException">
        /// value is equal to System.Double.NaN.
        /// </exception>
        public static TimeCode FromFrames(long frames, SmpteFrameRate rate)
        {
            var abs = FramesToAbsoluteTime(frames, rate);
            return new TimeCode(abs, rate);
        }

        /// <summary>
        /// Returns a TimeCode that represents a specified time, where the specification
        ///  is in units of ticks.
        /// </summary>
        /// <param name="ticks"> A number of ticks that represent a time.</param>
        /// <param name="rate">The Smpte framerate.</param>
        /// <returns>A TimeCode with a value of value.</returns>
        public static TimeCode FromTicks(long ticks, SmpteFrameRate rate)
        {
            var absoluteTime = Math.Pow(10, -7) * ticks;
            return new TimeCode(absoluteTime, rate);
        }

        /// <summary>
        /// Returns a TimeCode that represents a specified time, where the specification is 
        /// in units of absolute time.
        /// </summary>
        /// <param name="time">The absolute time in 100 nanosecond units.</param>
        /// <param name="rate">The SMPTE framerate.</param>
        /// <returns>A TimeCode.</returns>
        public static TimeCode FromAbsoluteTime(double time, SmpteFrameRate rate)
        {
            return new TimeCode(time, rate);
        }

        /// <summary>
        /// Validates that the string provided is in the correct format for SMPTE 12M time code.
        /// </summary>
        /// <param name="timeCode">String that is the time code.</param>
        /// <returns>True if this is a valid SMPTE 12M time code string.</returns>
        public static bool ValidateSmpte12MTimeCode(string timeCode)
        {
            var times = timeCode.Split(':', ';');

            var hours = Int32.Parse(times[0], CultureInfo.InvariantCulture);
            var minutes = Int32.Parse(times[1], CultureInfo.InvariantCulture);
            var seconds = Int32.Parse(times[2], CultureInfo.InvariantCulture);
            var frames = Int32.Parse(times[3], CultureInfo.InvariantCulture);

            if ((hours >= 24) || (minutes >= 60) || (seconds >= 60) || (frames >= 30))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates that the hexadecimal formatted integer provided is in the correct format for SMPTE 12M time code
        /// Time code is stored so that the hexadecimal value is read as if it were an integer value. 
        /// That is, the time code value 0x01133512 does not represent integer 18035986, rather it specifies 1 hour, 13 minutes, 35 seconds, and 12 frames.      
        /// </summary>
        /// <param name="windowsMediaTimeCode">Integer that is the time code stored in hexadecimal format.</param>
        /// <returns>True if this is a valid SMPTE 12M time code string.</returns>
        public static bool ValidateSmpte12MTimeCode(int windowsMediaTimeCode)
        {
            var timeCodeBytes = BitConverter.GetBytes(windowsMediaTimeCode);
            var timeCode = string.Format(CultureInfo.InvariantCulture, "{3:x2}:{2:x2}:{1:x2}:{0:x2}", timeCodeBytes[0], timeCodeBytes[1], timeCodeBytes[2], timeCodeBytes[3]);
            var times = timeCode.Split(':', ';');

            var hours = Int32.Parse(times[0], CultureInfo.InvariantCulture);
            var minutes = Int32.Parse(times[1], CultureInfo.InvariantCulture);
            var seconds = Int32.Parse(times[2], CultureInfo.InvariantCulture);
            var frames = Int32.Parse(times[3], CultureInfo.InvariantCulture);

            if ((hours >= 24) || (minutes >= 60) || (seconds >= 60) || (frames >= 30))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Parses a framerate value as double and converts it to a member of the SmpteFrameRate enumeration.
        /// </summary>
        /// <param name="rate">Double value of the framerate.</param>
        /// <returns>A SmpteFrameRate enumeration value that matches the incoming rates.</returns>
        public static SmpteFrameRate ParseFrameRate(double rate)
        {
            var rateRounded = (int)Math.Floor(rate);

            switch (rateRounded)
            {
                case 23: return SmpteFrameRate.Smpte2398;
                case 24: return SmpteFrameRate.Smpte24;
                case 25: return SmpteFrameRate.Smpte25;
                case 29: return SmpteFrameRate.Smpte2997NonDrop;
                case 30: return SmpteFrameRate.Smpte30;
                case 50: return SmpteFrameRate.Smpte25;
                case 60: return SmpteFrameRate.Smpte30;
                case 59: return SmpteFrameRate.Smpte2997NonDrop;
            }

            return SmpteFrameRate.Unknown;
        }

        /// <summary>
        /// Converts an absolute time and a frame rate to a formatted string.
        /// </summary>
        /// <param name="absoluteTime">Double precision floating point time in seconds.</param>
        /// <param name="frameRate">SMPTE frame rate enum.</param>
        /// <returns>A string that contains the correct format.</returns>
        public static string ConvertToString(double absoluteTime, SmpteFrameRate frameRate)
        {
            var timeCode = new TimeCode(absoluteTime, frameRate);
            return timeCode.ToString();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds the specified TimeCode to this instance.
        /// </summary>
        /// <param name="ts">A TimeCode.</param>
        /// <returns>A TimeCode that represents the value of this instance plus the value of ts.
        /// </returns>
        /// <exception cref="System.OverflowException">
        /// The resulting TimeCode is less than TimeCode.MinValue or greater than TimeCode.MaxValue.
        /// </exception>
        public TimeCode Add(TimeCode ts)
        {
            return this + ts;
        }

        /// <summary>
        ///  Compares this instance to a specified object and returns an indication of
        ///   their relative values.
        /// </summary>
        /// <param name="obj">An object to compare, or null.</param>
        /// <returns>
        ///  Value Condition -1 The value of this instance is less than the value of value.
        ///    0 The value of this instance is equal to the value of value. 1 The value
        ///    of this instance is greater than the value of value.-or- value is null.
        /// </returns>
        /// <exception cref="System.ArgumentException">
        ///  value is not a TimeCode.
        /// </exception>
        public int CompareTo(object obj)
        {
            if (!(obj is TimeCode))
            {
                throw new ArgumentException("Smpte12MOverflowException");
            }

            var time1 = (TimeCode)obj;

            if (this < time1)
            {
                return -1;
            }

            return this == time1 ? 0 : 1;
        }

        /// <summary>
        /// Compares this instance to a specified TimeCode object and returns
        /// an indication of their relative values.
        /// </summary>
        /// <param name="other"> A TimeCode object to compare to this instance.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance and value.Value
        /// Description A negative integer This instance is less than value. Zero This
        /// instance is equal to value. A positive integer This instance is greater than
        /// value.
        /// </returns>
        public int CompareTo(TimeCode other)
        {
            if (this < other)
            {
                return -1;
            }

            if (this == other)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        ///  Returns a value indicating whether this instance is equal to a specified
        ///  object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// True if value is a TimeCode object that represents the same time interval
        /// as the current TimeCode structure; otherwise, false.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TimeCode && Equals((TimeCode)obj);
        }

        /// <summary>
        /// Returns a value indicating whether this instance is equal to a specified
        ///     TimeCode object.
        /// </summary>
        /// <param name="other">An TimeCode object to compare with this instance.</param>
        /// <returns>true if obj represents the same time interval as this instance; otherwise, false.
        /// </returns>
        public bool Equals(TimeCode other)
        {
            return _absoluteTime.Equals(other._absoluteTime) && _frameRate == other._frameRate;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_absoluteTime.GetHashCode() * 397) ^ (int)_frameRate;
            }
        }

        /// <summary>
        /// Subtracts the specified TimeCode from this instance.
        /// </summary>
        /// <param name="ts">A TimeCode.</param>
        /// <returns>A TimeCode whose value is the result of the value of this instance minus the value of ts.</returns>
        /// <exception cref="OverflowException">The return value is less than TimeCode.MinValue or greater than TimeCode.MaxValue.</exception>
        public TimeCode Subtract(TimeCode ts)
        {
            return this - ts;
        }

        /// <summary>
        /// Returns the SMPTE 12M string representation of the value of this instance.
        /// </summary>
        /// <returns>
        /// A string that represents the value of this instance. The return value is
        ///     of the form: hh:mm:ss:ff for non-drop frame and hh:mm:ss;ff for drop frame code
        ///     with "hh" hours, ranging from 0 to 23, "mm" minutes
        ///     ranging from 0 to 59, "ss" seconds ranging from 0 to 59, and  "ff"  based on the 
        ///     chosen framerate to be used by the time code instance.
        /// </returns>
        public override string ToString()
        {
            return AbsoluteTimeToSmpte12M(_absoluteTime, _frameRate);
        }

        /// <summary>
        /// Outputs a string of the current time code in the requested framerate.
        /// </summary>
        /// <param name="rate">The SmpteFrameRate required for the string output.</param>
        /// <returns>SMPTE 12M formatted time code string converted to the requested framerate.</returns>
        public string ToString(SmpteFrameRate rate)
        {
            return AbsoluteTimeToSmpte12M(_absoluteTime, rate);
        }

        #endregion

        #region Private Static Methdos

        /// <summary>
        /// Converts a SMPTE timecode to absolute time.
        /// </summary>
        /// <param name="timeCode">The timecode to convert from.</param>
        /// <param name="rate">The <see cref="SmpteFrameRate"/> of the timecode.</param>
        /// <returns>A <see cref="double"/> with the absolute time.</returns>
        private static double Smpte12MToAbsoluteTime(string timeCode, SmpteFrameRate rate)
        {
            double absoluteTime = 0;

            switch (rate)
            {
                case SmpteFrameRate.Smpte2398:
                    absoluteTime = Smpte12M_23_98_ToAbsoluteTime(timeCode);
                    break;
                case SmpteFrameRate.Smpte24:
                    absoluteTime = Smpte12M_24_ToAbsoluteTime(timeCode);
                    break;
                case SmpteFrameRate.Smpte25:
                    absoluteTime = Smpte12M_25_ToAbsoluteTime(timeCode);
                    break;
                case SmpteFrameRate.Smpte2997Drop:
                    absoluteTime = Smpte12M_29_97_Drop_ToAbsoluteTime(timeCode);
                    break;
                case SmpteFrameRate.Smpte2997NonDrop:
                    absoluteTime = Smpte12M_29_97_NonDrop_ToAbsoluteTime(timeCode);
                    break;
                case SmpteFrameRate.Smpte30:
                    absoluteTime = Smpte12M_30_ToAbsoluteTime(timeCode);
                    break;
            }
            return absoluteTime;
        }

        /// <summary>
        /// Parses a timecode string for the different parts of the timecode.
        /// </summary>
        /// <param name="timeCode">The source timecode to parse.</param>
        /// <param name="hours">The Hours section from the timecode.</param>
        /// <param name="minutes">The Minutes section from the timecode.</param>
        /// <param name="seconds">The Seconds section from the timecode.</param>
        /// <param name="frames">The frames section from the timecode.</param>
        private static void ParseTimecodeString(string timeCode, out int hours, out int minutes, out int seconds, out int frames)
        {
            if (!ValidateTimecode.IsMatch(timeCode))
            {
                throw new FormatException("Smpte12MOverflowException");
            }

            var times = timeCode.Split(':', ';');

            hours = Int32.Parse(times[0], CultureInfo.InvariantCulture);
            minutes = Int32.Parse(times[1], CultureInfo.InvariantCulture);
            seconds = Int32.Parse(times[2], CultureInfo.InvariantCulture);
            frames = Int32.Parse(times[3], CultureInfo.InvariantCulture);

            if ((hours >= 24) || (minutes >= 60) || (seconds >= 60) || (frames >= 30))
            {
                throw new FormatException("Smpte12MOverflowException");
            }
        }

        /// <summary>
        /// Generates a string representation of the timecode.
        /// </summary>
        /// <param name="hours">The Hours section from the timecode.</param>
        /// <param name="minutes">The Minutes section from the timecode.</param>
        /// <param name="seconds">The Seconds section from the timecode.</param>
        /// <param name="frames">The frames section from the timecode.</param>
        /// <param name="dropFrame">Indicates whether the timecode is drop frame or not.</param>
        /// <returns>The timecode in string format.</returns>
        private static string FormatTimeCodeString(int hours, int minutes, int seconds, int frames, bool dropFrame)
        {
            var framesSeparator = ":";
            if (dropFrame)
            {
                framesSeparator = ";";
            }
            return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2:D2}", FormatTimeCodeStringNoFrames(hours, minutes, seconds), framesSeparator, frames);
        }

        /// <summary>
        /// Generates a string representation of the timecode.
        /// </summary>
        /// <param name="hours">The Hours section from the timecode.</param>
        /// <param name="minutes">The Minutes section from the timecode.</param>
        /// <param name="seconds">The Seconds section from the timecode.</param>
        /// <returns>The timecode in string format.</returns>
        private static string FormatTimeCodeStringNoFrames(int hours, int minutes, int seconds)
        {
            return new TimeSpan(hours, minutes, seconds).ToString();
        }

        /// <summary>
        /// Converts to Absolute time from SMPTE 12M 23.98.
        /// </summary>
        /// <param name="timeCode">The timecode to parse.</param>
        /// <returns>A <see cref="double"/> that contains the absolute duration.</returns>
        private static double Smpte12M_23_98_ToAbsoluteTime(string timeCode)
        {
            int hours, minutes, seconds, frames;

            ParseTimecodeString(timeCode, out hours, out minutes, out seconds, out frames);

            if (frames >= 24)
            {
                throw new FormatException("Smpte12MOverflowException");
            }

            return (1001 / 24000D) * (frames + (24 * seconds) + (1440 * minutes) + (86400 * hours));
        }

        /// <summary>
        /// Converts to Absolute time from SMPTE 12M 24.
        /// </summary>
        /// <param name="timeCode">The timecode to parse.</param>
        /// <returns>A <see cref="double"/> that contains the absolute duration.</returns>
        private static double Smpte12M_24_ToAbsoluteTime(string timeCode)
        {
            int hours, minutes, seconds, frames;

            ParseTimecodeString(timeCode, out hours, out minutes, out seconds, out frames);

            if (frames >= 24)
            {
                throw new FormatException("Smpte12MOverflowException");
            }

            return (1 / 24D) * (frames + (24 * seconds) + (1440 * minutes) + (86400 * hours));
        }

        /// <summary>
        /// Converts to Absolute time from SMPTE 12M 25.
        /// </summary>
        /// <param name="timeCode">The timecode to parse.</param>
        /// <returns>A <see cref="double"/> that contains the absolute duration.</returns>
        private static double Smpte12M_25_ToAbsoluteTime(string timeCode)
        {
            int hours, minutes, seconds, frames;

            ParseTimecodeString(timeCode, out hours, out minutes, out seconds, out frames);

            if (frames >= 25)
            {
                throw new FormatException("Smpte12MOverflowException");
            }

            return (1 / 25D) * (frames + (25 * seconds) + (1500 * minutes) + (90000 * hours));
        }

        /// <summary>
        /// Converts to Absolute time from SMPTE 12M 29.97 Drop frame.
        /// </summary>
        /// <param name="timeCode">The timecode to parse.</param>
        /// <returns>A <see cref="double"/> that contains the absolute duration.</returns>
        private static double Smpte12M_29_97_Drop_ToAbsoluteTime(string timeCode)
        {
            int hours, minutes, seconds, frames;

            ParseTimecodeString(timeCode, out hours, out minutes, out seconds, out frames);

            if (frames >= 30)
            {
                throw new FormatException("Smpte12MOverflowException");
            }

            return (1001 / 30000D) * (frames + (30 * seconds) + (1798 * minutes) + ((2 * (minutes / 10)) + (107892 * hours)));
        }

        /// <summary>
        /// Converts to Absolute time from SMPTE 12M 29.97 Non Drop.
        /// </summary>
        /// <param name="timeCode">The timecode to parse.</param>
        /// <returns>A <see cref="double"/> that contains the absolute duration.</returns>
        private static double Smpte12M_29_97_NonDrop_ToAbsoluteTime(string timeCode)
        {
            int hours, minutes, seconds, frames;

            ParseTimecodeString(timeCode, out hours, out minutes, out seconds, out frames);

            if (frames >= 30)
            {
                throw new FormatException("Smpte12MOverflowException");
            }

            return (1001 / 30000D) * (frames + (30 * seconds) + (1800 * minutes) + (108000 * hours));
        }

        /// <summary>
        /// Converts to Absolute time from SMPTE 12M 30.
        /// </summary>
        /// <param name="timeCode">The timecode to parse.</param>
        /// <returns>A <see cref="double"/> that contains the absolute duration.</returns>
        private static double Smpte12M_30_ToAbsoluteTime(string timeCode)
        {
            int hours, minutes, seconds, frames;

            ParseTimecodeString(timeCode, out hours, out minutes, out seconds, out frames);

            if (frames >= 30)
            {
                throw new FormatException("Smpte12MOverflowException");
            }

            return (1 / 30D) * (frames + (30 * seconds) + (1800 * minutes) + (108000 * hours));
        }

        /// <summary>
        /// Converts to SMPTE 12M.
        /// </summary>
        /// <param name="absoluteTime">The absolute time to convert from.</param>
        /// <param name="rate">The SMPTE frame rate.</param>
        /// <returns>A string in SMPTE 12M format.</returns>
        private static string AbsoluteTimeToSmpte12M(double absoluteTime, SmpteFrameRate rate)
        {
            var timeCode = string.Empty;

            switch (rate)
            {
                case SmpteFrameRate.Smpte2398:
                    timeCode = AbsoluteTimeToSmpte12M_23_98fps(absoluteTime);
                    break;
                case SmpteFrameRate.Smpte24:
                    timeCode = AbsoluteTimeToSmpte12M_24fps(absoluteTime);
                    break;
                case SmpteFrameRate.Smpte25:
                    timeCode = AbsoluteTimeToSmpte12M_25fps(absoluteTime);
                    break;
                case SmpteFrameRate.Smpte2997Drop:
                    timeCode = AbsoluteTimeToSmpte12M_29_97_Drop(absoluteTime);
                    break;
                case SmpteFrameRate.Smpte2997NonDrop:
                    timeCode = AbsoluteTimeToSmpte12M_29_97_NonDrop(absoluteTime);
                    break;
                case SmpteFrameRate.Smpte30:
                    timeCode = AbsoluteTimeToSmpte12M_30fps(absoluteTime);
                    break;
            }

            return timeCode;
        }

        /// <summary>
        /// Returns the number of frames.
        /// </summary>
        /// <param name="absoluteTime">The absolute time to use for parsing from.</param>
        /// <param name="rate">The SMPTE frame rate to use for the conversion.</param>
        /// <returns>A <see cref="long"/> with the number of frames.</returns>
        private static long AbsoluteTimeToFrames(double absoluteTime, SmpteFrameRate rate)
        {
            switch (rate)
            {
                case SmpteFrameRate.Smpte2398:
                    return (long)Math.Floor(24 * (1000 / 1001D) * absoluteTime);
                case SmpteFrameRate.Smpte24:
                    return Convert.ToInt64(24 * absoluteTime);
                case SmpteFrameRate.Smpte25:
                    return Convert.ToInt64(25 * absoluteTime);
                case SmpteFrameRate.Smpte2997Drop:
                case SmpteFrameRate.Smpte2997NonDrop:
                    return (long)Math.Floor(30 * (1000 / 1001D) * absoluteTime);
                case SmpteFrameRate.Smpte30:
                    return Convert.ToInt64(30 * absoluteTime);
                default:
                    return Convert.ToInt64(30 * absoluteTime);
            }
        }

        /// <summary>
        /// Returns the absolute time.
        /// </summary>
        /// <param name="frames">The number of frames.</param>
        /// <param name="rate">The SMPTE frame rate to use for the conversion.</param>
        /// <returns>The absolute time.</returns>
        private static double FramesToAbsoluteTime(long frames, SmpteFrameRate rate)
        {
            switch (rate)
            {
                case SmpteFrameRate.Smpte2398:
                    return Math.Ceiling(frames / 24D / (1000 / 1001D));
                case SmpteFrameRate.Smpte24:
                    return Math.Ceiling(frames / 24D);
                case SmpteFrameRate.Smpte25:
                    return Math.Ceiling(frames / 25D);
                case SmpteFrameRate.Smpte2997Drop:
                case SmpteFrameRate.Smpte2997NonDrop:
                    return frames / 30D / (1000 / 1001D);
                case SmpteFrameRate.Smpte30:
                    return frames / 30D;
                default:
                    return frames / 30D;
            }
        }

        /// <summary>
        /// Returns the SMPTE 12M 23.98 timecode.
        /// </summary>
        /// <param name="absoluteTime">The absolute time to convert from.</param>
        /// <returns>A string that contains the correct format.</returns>
        private static string AbsoluteTimeToSmpte12M_23_98fps(double absoluteTime)
        {
            var framecount = AbsoluteTimeToFrames(absoluteTime, SmpteFrameRate.Smpte2398);
            var hours = Convert.ToInt32((framecount / 86400) % 24);
            var minutes = Convert.ToInt32((framecount - (86400 * hours)) / 1440);
            var seconds = Convert.ToInt32((framecount - (1440 * minutes) - (86400 * hours)) / 24);
            var frames = Convert.ToInt32(framecount - (24 * seconds) - (1440 * minutes) - (86400 * hours));

            return FormatTimeCodeString(hours, minutes, seconds, frames, false);
        }

        /// <summary>
        /// Converts to SMPTE 12M 24fps.
        /// </summary>
        /// <param name="absoluteTime">The absolute time to convert from.</param>
        /// <returns>A string that contains the correct format.</returns>
        private static string AbsoluteTimeToSmpte12M_24fps(double absoluteTime)
        {
            var framecount = AbsoluteTimeToFrames(absoluteTime, SmpteFrameRate.Smpte24);
            var hours = Convert.ToInt32((framecount / 86400) % 24);
            var minutes = Convert.ToInt32((framecount - (86400 * hours)) / 1440);
            var seconds = Convert.ToInt32(((framecount - (1440 * minutes) - (86400 * hours)) / 24));
            var frames = Convert.ToInt32(framecount - (24 * seconds) - (1440 * minutes) - (86400 * hours));

            return FormatTimeCodeString(hours, minutes, seconds, frames, false);
        }

        /// <summary>
        /// Converts to SMPTE 12M 25fps.
        /// </summary>
        /// <param name="absoluteTime">The absolute time to convert from.</param>
        /// <returns>A string that contains the correct format.</returns>
        private static string AbsoluteTimeToSmpte12M_25fps(double absoluteTime)
        {
            var framecount = AbsoluteTimeToFrames(absoluteTime, SmpteFrameRate.Smpte25);
            var hours = Convert.ToInt32((framecount / 90000) % 24);
            var minutes = Convert.ToInt32((framecount - (90000 * hours)) / 1500);
            var seconds = Convert.ToInt32(((framecount - (1500 * minutes) - (90000 * hours)) / 25));
            var frames = Convert.ToInt32(framecount - (25 * seconds) - (1500 * minutes) - (90000 * hours));

            return FormatTimeCodeString(hours, minutes, seconds, frames, false);
        }

        /// <summary>
        /// Converts to SMPTE 12M 29.97fps Drop.
        /// </summary>
        /// <param name="absoluteTime">The absolute time to convert from.</param>
        /// <returns>A string that contains the correct format.</returns>
        private static string AbsoluteTimeToSmpte12M_29_97_Drop(double absoluteTime)
        {
            var framecount = AbsoluteTimeToFrames(absoluteTime, SmpteFrameRate.Smpte2997Drop);
            var hours = (int)((framecount / 107892) % 24);
            var minutes = Convert.ToInt32((framecount + (2 * ((int)((framecount - (107892 * hours)) / 1800))) - (2 * ((int)((framecount - (107892 * hours)) / 18000))) - (107892 * hours)) / 1800);
            var seconds = Convert.ToInt32((framecount - (1798 * minutes) - (2 * ((int)(minutes / 10D))) - (107892 * hours)) / 30);
            var frames = Convert.ToInt32(framecount - (30 * seconds) - (1798 * minutes) - (2 * ((int)(minutes / 10D))) - (107892 * hours));

            return FormatTimeCodeString(hours, minutes, seconds, frames, true);
        }

        /// <summary>
        /// Converts to SMPTE 12M 29.97fps Non Drop.
        /// </summary>
        /// <param name="absoluteTime">The absolute time to convert from.</param>
        /// <returns>A string that contains the correct format.</returns>
        private static string AbsoluteTimeToSmpte12M_29_97_NonDrop(double absoluteTime)
        {
            var framecount = AbsoluteTimeToFrames(absoluteTime, SmpteFrameRate.Smpte2997NonDrop);
            var hours = Convert.ToInt32((framecount / 108000) % 24);
            var minutes = Convert.ToInt32((framecount - (108000 * hours)) / 1800);
            var seconds = Convert.ToInt32(((framecount - (1800 * minutes) - (108000 * hours)) / 30));
            var frames = Convert.ToInt32(framecount - (30 * seconds) - (1800 * minutes) - (108000 * hours));

            return FormatTimeCodeString(hours, minutes, seconds, frames, false);
        }

        /// <summary>
        /// Converts to SMPTE 12M 30fps.
        /// </summary>
        /// <param name="absoluteTime">The absolute time to convert from.</param>
        /// <returns>A string that contains the correct format.</returns>
        private static string AbsoluteTimeToSmpte12M_30fps(double absoluteTime)
        {
            var framecount = AbsoluteTimeToFrames(absoluteTime, SmpteFrameRate.Smpte30);
            var hours = Convert.ToInt32((framecount / 108000) % 24);
            var minutes = Convert.ToInt32((framecount - (108000 * hours)) / 1800);
            var seconds = Convert.ToInt32(((framecount - (1800 * minutes) - (108000 * hours)) / 30));
            var frames = Convert.ToInt32(framecount - (30 * seconds) - (1800 * minutes) - (108000 * hours));

            return FormatTimeCodeString(hours, minutes, seconds, frames, false);
        }
        #endregion
    }
}
