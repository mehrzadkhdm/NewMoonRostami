using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;

namespace MySweph
{
    public static class Sweph
    {
        [DllImport("swedll32.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_set_ephe_path")]
        private extern static void ext_swe_set_ephe_path(String path);
        public static void setEphePath(String path)
        {
            ext_swe_set_ephe_path(path);
        }
        //
        //
        //
        [DllImport("swedll32.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_julday")]
        private extern static double ext_swe_julday(int year, int month, int day, double hour, int gregflag);
        public static double getJD(int year, int month, int day, double hour, int cal)
        {
            return ext_swe_julday(year, month, day, hour, cal);
        }

        [DllImport("swedll32.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_deltat")]
        private extern static double ext_swe_deltat(double jd);
        public static double deltaT(double jd)
        {
            return ext_swe_deltat(jd);
        }
        [DllImport("swedll32.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_revjul")]
        private extern static double ext_swe_revjul(double tjd, int gregflag, ref int year, ref int month, ref int day, ref double hour);

        private static double swe_revjul(double tjd, int gregflag, ref int year, ref int month, ref int day, ref double hour)
        {
            return ext_swe_revjul(tjd, gregflag, ref year, ref month, ref day, ref hour);
        }
        public static double GetRevJul(double tjd, int gregflag, ref int year, ref int month, ref int day, ref double hour)
        {
            return swe_revjul(tjd, gregflag, ref year, ref month, ref day, ref hour);
        }


        [DllImport("swedll32.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_set_topo")]
        private extern static void ext_swe_set_topo(double geolon, double geolat, double altitude);

        public static void setTopo(double geolon, double geolat, double altitude)
        {
            ext_swe_set_topo(geolon, geolat, altitude);
        }
        /// <summary>
        /// Returns the daynumber for a given Julain day number
        /// </summary>
        /// <param name="jdnr">The Julian Day</param>
        /// <param name="cal">Calendar used</param>
        /// <returns>The day number</returns>
        public static int getDayFromJd(double jdnr, int cal)
        {
            int day = 0, month = 0, year = 0;
            double hour = 0;
            swe_revjul(jdnr, cal, ref year, ref month, ref day, ref hour);
            return day;
        }


        [DllImport("swedll32.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_calc")]
        private extern static long ext_swe_calc(double jdnr, int index, int y, double[] x, String serr);
        /// <summary>
        /// Access Obliquity
        /// </summary>
        /// <param name="jdnr">Julian Day number</param>
        /// <returns>Mean obliquity</returns>
        public static double getObliquity(double jdnr)
        {
            double[] x = new double[6];
            String serr = "";
            long iflgret = ext_swe_calc(jdnr, Constants.SE_ECL_NUT, 0, x, serr);
            return x[1];  // mean obliquity
        }

        [DllImport("swedll32.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_calc_ut")]
        private extern static int ext_swe_calc_ut(double jdnr, int ipl, int iflag, double[] xx, String serr);

        /// <summary>
        /// Calculate planetary position
        /// </summary>
        /// <param name="ipl">Index of planetary body</param>
        /// <param name="jdnr">Julain day</param>
        /// <returns>Array with 6 doubles: 0:longitude, 1:latitude, 2:distance,3:speel in longitude, 
        ///          4: speed in latitude, 5: speed in distance </returns>
        public static double[] getPlanet(int ipl, double jdnr)
        {
            //   String ephePath = "Q:\\sweph\\";
            //   Sweph.setEphePath(ephePath);
            double[] xx2 = new double[8];
            double[] xx = new double[6];
            String serr = "";
            int iflag = Constants.SEFLG_SPEED;
            int iflgret = ext_swe_calc_ut(jdnr, ipl, iflag, xx, serr);
            for (int i = 0; i < 6; i++)
            {
                xx2[i] = xx[i];
            }
            iflag = Constants.SEFLG_SWIEPH | Constants.SEFLG_SPEED | Constants.SEFLG_EQUATORIAL;
            iflgret = ext_swe_calc_ut(jdnr, ipl, iflag, xx, serr);
            xx2[6] = xx[0];
            xx2[7] = xx[1];
            return xx2;
        }
        public static double[] getPlanet(int ipl, double jdnr, int iflag)
        {
            //   String ephePath = "Q:\\sweph\\";
            //   Sweph.setEphePath(ephePath);
            double[] xx2 = new double[8];
            double[] xx = new double[6];
            String serr = "";
            iflag |= Constants.SEFLG_SPEED;
            int iflgret = ext_swe_calc_ut(jdnr, ipl, iflag, xx, serr);
            //for (int i = 0; i < 6; i++)
            //{
            //    xx2[i] = xx[i];
            //}
            //iflag = Constants.SEFLG_SWIEPH | Constants.SEFLG_SPEED | Constants.SEFLG_EQUATORIAL;
            //iflgret = ext_swe_calc_ut(jdnr, ipl, iflag, xx, serr);
            //xx2[6] = xx[0];
            //xx2[7] = xx[1];
            return xx;
        }

        [DllImport("swedll32.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_pheno_ut")]
        private extern static int ext_swe_pheno_ut(double jdnr, int ipl, int iflag, double[] xx, String serr);
        public static double[] getPhenoUt(int ipl, double jdnr, int iflag)
        {
            double[] xx = new double[20];
            String serr = "";
            int iflgret = ext_swe_pheno_ut(jdnr, ipl, iflag, xx, serr);
            return xx;
        }

        [DllImport("swedll32.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_azalt")]
        private extern static int ext_swe_azalt(double jdut, int cflag, double[] pos, double press, double temp, double[] coord, double[] xza);
        public static double[] getAltAz(int ipl, double jdut, double geolon, double geolat, double altitude, double press, double temp)
        {
            double[] xza = new double[3];
            double[] aa = Sweph.getPlanet(ipl, jdut, Constants.SEFLG_SWIEPH | Constants.SEFLG_TOPOCTR);
            double[] coord = new double[3];
            coord[0] = aa[0]; coord[1] = aa[1]; coord[2] = aa[2];
            double[] mypos = new double[] { geolon, geolat, altitude };
            int ans = ext_swe_azalt(jdut, Constants.SE_ECL2HOR, mypos, press, temp, coord, xza);
            return xza;

        }
        //
        //
        //
        [DllImport("swedll32.dll", CharSet = CharSet.Ansi, EntryPoint = "swe_rise_trans")]
        private extern static int ext_swe_rise_trans(double jdut, int ipl, String star, int eflag, int rsmi, double[] pos, double press, double temp, ref double  rtime, String serr);
        public static double getRiseSetTime(int ipl, double jdut, double geolon, double geolat, double altitude, double press, double temp, int eflag, int rsmi)
        {
            String serr = "";
            String star = "";
            double rtime = 0;
            double[] mypos = new double[] { geolon, geolat, altitude };

            int ans = ext_swe_rise_trans(jdut, ipl, star, eflag, rsmi, mypos, press, temp,ref rtime, serr);
            return rtime;
        }
    }
}
