using System;
using System.Text;
using System.Text.RegularExpressions;

namespace MySofaJpl
{

    public class Sexagesimal : System.IComparable<MySofaJpl.Sexagesimal>, System.IComparable, System.IEquatable<MySofaJpl.Sexagesimal>, System.IFormattable
    {

        private double degrees;
        private double minutes;
        private int modulus;
        private double resolution;
        private int revolutions;
        private double seconds;
        private char sign;

        public double Degrees
        {
            get
            {
                return degrees;
            }
        }

        public double Hours
        {
            get
            {
                return Degrees;
            }
        }

        public double Minutes
        {
            get
            {
                return minutes;
            }
        }

        public double Resolution
        {
            get
            {
                return resolution;
            }
        }

        public int Revolutions
        {
            get
            {
                return revolutions;
            }
        }

        public double Seconds
        {
            get
            {
                return seconds;
            }
        }

        public char Sign
        {
            get
            {
                return sign;
            }
        }

        public Sexagesimal(double degrees, double resolution, int modulus, double stepSeconds)
        {
            sign = '+';
            if (resolution <= 0.0)
                throw new System.ArgumentOutOfRangeException("Sexagesimal constructor resolution argument was <= 0.");
            if (modulus < 0)
                throw new System.ArgumentOutOfRangeException("Sexagesimal constructor modulus argument was < 0.");
            if (degrees < 0.0)
            {
                sign = '-';
                degrees = -degrees;
            }
            this.resolution = resolution;
            this.modulus = modulus;
            double d1 = System.Math.Floor((degrees * resolution) + 0.5);
            double d2 = resolution / 60.0;
            double d3 = resolution / 3600.0;
            this.degrees = d1 / resolution;
            d1 -= System.Math.Floor(this.degrees) * resolution;
            if (d1 != 0.0)
            {
                minutes = d1 / d2;
                d1 -= System.Math.Floor(minutes) * d2;
                double d4 = 0.0;
                if (stepSeconds > 0.0)
                    d4 = System.Math.Floor((stepSeconds * d3) + 0.5);
                seconds = (d1 + d4) / d3;
            }
            if (modulus > 0)
            {
                revolutions = (int)this.degrees / modulus * (sign == '+' ? 1 : -1);
                this.degrees %= (double)modulus;
            }
        }

        public Sexagesimal(double degrees, double resolution, int modulus) : this(degrees, resolution, modulus, 0.0)
        {
        }

        public Sexagesimal(double degrees, double resolution) : this(degrees, resolution, 0, 0.0)
        {
        }

        public int CompareTo(object obj)
        {
            if (obj == null)
                return 1;
            if (GetType() != obj.GetType())
                throw new System.ArgumentException("Sexagesimal CompareTo(obj): type mismatch.");
            return CompareTo((MySofaJpl.Sexagesimal)obj);
        }

        public int CompareTo(MySofaJpl.Sexagesimal other)
        {
            if (other == null)
                return 1;
            double d = ToDouble();
            return d.CompareTo(other.ToDouble());
        }

        public int DegreeDecimals()
        {
            int i = (int)(System.Math.Log10(Resolution) + 0.8);
            if (i <= 0)
                return 0;
            return i;
        }

        public bool Equals(MySofaJpl.Sexagesimal other)
        {
            return CompareTo(other) == 0;
        }

        private string FormatDegrees(bool zeroFlag, int width, bool decimalFormat)
        {
            string s;

            int i1 = 0;
            if (decimalFormat)
            {
                i1 = DegreeDecimals();
                s = System.String.Format("{0:f" + i1.ToString() + "}", Degrees);
            }
            else
            {
                int i2 = (int)Degrees;
                s = i2.ToString();
            }
            if (width != 0)
            {
                width += i1 + (i1 > 0 ? 1 : 0);
                if (zeroFlag)
                    s = s.PadLeft(width, '0');
                else
                    s = s.PadLeft(width);
            }
            return s;
        }

        private string FormatMinutes(bool zeroFlag, int width, bool decimalFormat)
        {
            string s;

            int i1 = 0;
            if (decimalFormat)
            {
                i1 = MinuteDecimals();
                s = System.String.Format("{0:f" + i1.ToString() + "}", Minutes);
            }
            else
            {
                int i2 = (int)Minutes;
                s = i2.ToString();
            }
            if (width != 0)
            {
                width += i1 + (i1 > 0 ? 1 : 0);
                if (zeroFlag)
                    s = s.PadLeft(width, '0');
                else
                    s = s.PadLeft(width);
            }
            return s;
        }

        private string FormatSeconds(bool zeroFlag, int width)
        {
            int i = SecondDecimals();
            string s = System.String.Format("{0:f" + i.ToString() + "}", Seconds);
            if (width != 0)
            {
                width += i + (i > 0 ? 1 : 0);
                if (zeroFlag)
                    s = s.PadLeft(width, '0');
                else
                    s = s.PadLeft(width);
            }
            return s;
        }

        private string formatSign(bool spaceFlag, bool plusFlag)
        {
            string s = "";
            if (plusFlag)
            {
                char ch = Sign;
                s = ch.ToString();
            }
            else if (spaceFlag)
            {
                if (Sign == '-')
                    s = "-";
                else
                    s = " ";
            }
            else if (Sign == '-')
            {
                s = "-";
            }
            return s;
        }

        public int MinuteDecimals()
        {
            int i = (int)System.Math.Log10(Resolution) - 1;
            if (i <= 0)
                return 0;
            return i;
        }

        public int SecondDecimals()
        {
            int i = (int)(System.Math.Log10(Resolution) - 2.8);
            if (i <= 0)
                return 0;
            return i;
        }

        public double ToDouble()
        {
            return (System.Math.Floor(Hours) + (System.Math.Floor(Minutes) / 60.0) + (Seconds / 3600.0)) * (sign == '+' ? 1.0 : -1.0);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            if ((format == "") || (format == null))
            {
                format = "a ";
            }
            string pattern = "^([ +0]){0,2}([1-9][0-9]*)?([a-zA-Z])(.{0,4})$";
            Match match = Regex.Match(format, pattern);
            if (!match.Success)
            {
                throw new ArgumentException("Sexagesimal.ToString() received invalid format string.");
            }
            GroupCollection groups = match.Groups;
            if (groups[1].Captures.Count > 2)
            {
                throw new ArgumentException("Sexagesimal format string has too many flag characters.");
            }
            bool spaceFlag = false;
            bool plusFlag = false;
            bool zeroFlag = false;
            for (int i = 0; i < groups[1].Captures.Count; i++)
            {
                string str7 = groups[1].Captures[i].Value;
                if (str7 == null)
                {
                    goto Label_00C2;
                }
                if (!(str7 == " "))
                {
                    if (str7 == "+")
                    {
                        goto Label_00B8;
                    }
                    if (str7 == "0")
                    {
                        goto Label_00BD;
                    }
                    goto Label_00C2;
                }
                spaceFlag = true;
                continue;
            Label_00B8:
                plusFlag = true;
                continue;
            Label_00BD:
                zeroFlag = true;
                continue;
            Label_00C2:
                throw new ArgumentException("Sexagesimal.ToString() control string has invalid flag character.");
            }
            if (spaceFlag && plusFlag)
            {
                throw new ArgumentException("Sexagesimal.ToString() control string invalid: ' ' and '+' are both present.");
            }
            int width = 0;
            if (groups[2].Success)
            {
                width = int.Parse(groups[2].Value);
            }
            string str2 = "";
            string str3 = "";
            string str4 = "";
            string str5 = "";
            if (groups[4].Success)
            {
                string str6 = groups[4].Value;
                if ((str6.Length == 2) || (str6.Length > 4))
                {
                    throw new ArgumentException("Sexagesimal ToString(): 0, 1, 3, or 4 characters must follow the format letter.");
                }
                if (str6.Length >= 3)
                {
                    str2 = str6[0].ToString();
                    str3 = str6[1].ToString();
                    str4 = str6[2].ToString();
                }
                if (str6.Length == 1)
                {
                    str5 = str6[0].ToString();
                }
                if (str6.Length == 4)
                {
                    str5 = str6[3].ToString();
                }
            }
            StringBuilder builder = new StringBuilder(20);
            builder.Append(this.formatSign(spaceFlag, plusFlag));
            switch (char.ToLower(groups[3].Value[0]))
            {
                case 'a':
                case 'g':
                    goto Label_0253;

                case 'b':
                    goto Label_02E6;

                case 'c':
                    builder.Append(this.FormatDegrees(zeroFlag, width, true));
                    builder.Append(str2);
                    goto Label_0471;

                case 'd':
                    goto Label_0363;

                case 'e':
                    if ((((int)this.degrees) != 0) || (this.resolution < 2.0))
                    {
                        goto Label_02E6;
                    }
                    builder.Append(this.FormatMinutes(zeroFlag, width, true));
                    builder.Append(str3);
                    goto Label_0471;

                case 'f':
                    if (((((int)this.degrees) != 0) || (((int)this.minutes) != 0)) || (this.resolution < 120.0))
                    {
                        goto Label_0363;
                    }
                    builder.Append(this.FormatSeconds(zeroFlag, width));
                    builder.Append(str4);
                    goto Label_0471;

                default:
                    throw new ArgumentException("Sexagesimal.ToString(): requested format letter not implemented.");
            }
        Label_0253:
            builder.Append(this.FormatDegrees(zeroFlag, width, false));
            builder.Append(str2);
            if (this.Resolution >= 2.0)
            {
                builder.Append(str5);
                builder.Append(this.FormatMinutes(true, 2, false));
                builder.Append(str3);
                if (this.Resolution >= 120.0)
                {
                    builder.Append(str5);
                    builder.Append(this.FormatSeconds(true, 2));
                    builder.Append(str4);
                }
            }
            goto Label_0471;
        Label_02E6:
            builder.Append(this.FormatDegrees(zeroFlag, width, false));
            builder.Append(str2);
            if (this.Resolution >= 2.0)
            {
                builder.Append(str5);
                builder.Append(this.FormatMinutes(true, 2, true));
                builder.Append(str3);
            }
            goto Label_0471;
        Label_0363:
            if ((((int)this.degrees) != 0) || (this.resolution < 2.0))
            {
                goto Label_0253;
            }
            builder.Append(this.FormatMinutes(zeroFlag, width, false));
            builder.Append(str3);
            if (this.Resolution >= 120.0)
            {
                builder.Append(str5);
                builder.Append(this.FormatSeconds(true, 2));
                builder.Append(str4);
            }
        Label_0471:
            return builder.ToString();
        }

        public override bool Equals(object obj)
        {
            if ((obj == null) || (GetType() != obj.GetType()))
                return false;
            return CompareTo((MySofaJpl.Sexagesimal)obj) == 0;
        }

        public override int GetHashCode()
        {
            double d = ToDouble();
            return d.GetHashCode();
        }

        public override string ToString()
        {
            return System.String.Format("{0:+g }", this);
        }

    } // class Sexagesimal

}

