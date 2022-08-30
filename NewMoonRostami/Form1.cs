using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using MySofaJpl;
using MySweph;
using AAPlus;
using myAstro;
using MyCalendar;
using GMap.NET;

namespace NewMoonRostami
{
	public partial class Form1 : Form
	{
		public double jd;
		public int myObject = 0;
		public double longitude, latitude, altitude, pressure, temperature;
		public double NewMoonJD;
		public double RAD = Math.PI / 180.0;
		public double DEG = 180.0 / Math.PI;
		public double irm, irm1 = 0.0 , irm2 = 0.0 , irm3 = 0.0 ;
		public double nextKK = 0;
        public StringBuilder strb, tb2;

		public Form1 ()
		{
			InitializeComponent ();
		}

		private void Form1_Load (object sender, EventArgs e)
		{
			Sweph.setEphePath (Application.StartupPath + @"\ephe");
			longitude = 54.0;
			latitude = 32.0;
			altitude = 300.0;
			pressure = 1010.0;
			temperature = 15.0;
			strb = new StringBuilder ();
            tb2 = new StringBuilder();
			UpdateCalendar ();
			UpdateLongLatLabels ();

		}

		public void Myrevjul (double jd, int cal, out int yy, out int mm, out int dd, out double hh)
		{
			double u0, u1, u2, u3, u4;
			u0 = jd + 32082.5;
			if (cal == Constants.SE_GREG_CAL) {
				u1 = u0 + Math.Floor (u0 / 36525.0) - Math.Floor (u0 / 146100.0) - 38.0;
				if (jd >= 1830691.5)
					u1 += 1;
				u0 = u0 + Math.Floor (u1 / 36525.0) - Math.Floor (u1 / 146100.0) - 38.0;
			}
			u2 = Math.Floor (u0 + 123.0);
			u3 = Math.Floor ((u2 - 122.2) / 365.25);
			u4 = Math.Floor ((u2 - Math.Floor (365.25 * u3)) / 30.6001);
			mm = (int)(u4 - 1.0);
			if (mm > 12)
				mm -= 12;
			dd = (int)(u2 - Math.Floor (365.25 * u3) - Math.Floor (30.6001 * u4));
			yy = (int)(u3 + Math.Floor ((u4 - 2.0) / 12.0) - 4800);
			hh = (jd - Math.Floor (jd + 0.5) + 0.5) * 24.0;
		}

		private void button1_Click (object sender, EventArgs e)
		{
			longitude = 52.5;
			latitude = 29.6;
			UpdateLongLatLabels ();
		}

		private void UpdateLongLatLabels ()
		{
			label1.Text = "Longitude = " + longitude.ToString ();
			label2.Text = "Latitude = " + latitude.ToString ();
		}

        private void button8_Click(object sender, EventArgs e)
        {
			longitude = -1.5;
			latitude = 52.00;
			altitude = 300;

			longitude = -8;
			latitude = 32.00;
			altitude = 448;

			longitude = 26;
			latitude = -29.00;
			altitude = 1294;
			UpdateLongLatLabels();
		}

        private void button5_Click (object sender, EventArgs e)
		{
			longitude = 51.41;
			latitude = 35.71;
			UpdateLongLatLabels ();
		}

		private void button4_Click (object sender, EventArgs e)
		{
			longitude = 51.67;
			latitude = 32.68;
			UpdateLongLatLabels ();
		}

		private void button3_Click (object sender, EventArgs e)
		{
			longitude = 54.0;
			latitude = 32.0;
			UpdateLongLatLabels ();
		}

		private void button6_Click (object sender, EventArgs e)
		{
			longitude = 55.5;
			latitude = 28.0;
			UpdateLongLatLabels ();
		}

		private void button2_Click (object sender, EventArgs e)
		{
			longitude = 52.0;
			latitude = 36.5;
			UpdateLongLatLabels ();
		}

		private void dateTimePicker1_ValueChanged (object sender, EventArgs e)
		{
			UpdateCalendar ();
		}

		void UpdateCalendar ()
		{
			int yy = dateTimePicker1.Value.Year;
			int mm = dateTimePicker1.Value.Month;
			int dd = dateTimePicker1.Value.Day;
			double yyf = yy + dateTimePicker1.Value.DayOfYear / 365.25;
			double kk = Math.Round (MoonPhases.K (yyf));
			NewMoonJD = MoonPhases.TruePhase (kk);
			jd = Sweph.getJD (yy, mm, dd, 0, Constants.SE_GREG_CAL);
			label3.Text = "JD = " + jd.ToString ();
			double hh;
			nextKK = kk;
			Myrevjul (NewMoonJD, Constants.SE_GREG_CAL, out yy, out mm, out dd, out hh);
			label4.Text = "Nearest new moon = " + dd.ToString () + "/" + mm.ToString () + "/" + yy.ToString () + " , " + hh.ToString ("##.###") + " GMT  ( k= " + kk.ToString () + " )";
		}

		private void button7_Click (object sender, EventArgs e)
		{
			int iflag = 0;
			iflag |= Constants.SEFLG_TOPOCTR;
			iflag |= Constants.SEFLG_SWIEPH;
			double[] altazS, altazM, MoonDetail;
			double ArcV, W, V=0.0, q, Lag, ArcVg, Lst, Hs, Hm, hs, hm, dh, dA;
            bool NewMoonFound, FirstDayTry = true, MaximumFound = false, NewMoonVisibleByEye, NewMoonVisibleByTelescope;
			double sinphi = Math.Sin (RAD * latitude);
			double cosphi = Math.Cos (RAD * latitude);
			double jdTry = NewMoonJD, JDMax;
			int yy1, mm1, dd1, yy2, mm2, dd2;
			double irmMax = 0.0, irmBestTime;
			double hh1, hh2, sset, mset, srise, mrise, bestTime;
			double j1 = 0.0, j2;
			double tz = 0.0;
			string ViewDeviceDay, ViewDeviceEvening, OdehVisibility;
            int cntM = 1, cntY = 1430;
            strb.Clear(); tb2.Clear();
			if (checkBox1.Checked) {
				tz = 3.5;
				if (checkBox2.Checked)
					tz = 4.5;
			}
			//
			double djd = 1.0 / 60.0 / 24.0;  // 5 minutes
            double jdp;
			//for (double kkf=111; kkf<303; kkf++) {
			for (double kkf=nextKK-1; kkf<nextKK+1; kkf++) {
				NewMoonJD = MoonPhases.TruePhase (kkf);
				//jd = Sweph.getJD (yy, mm, dd, 0, Constants.SE_GREG_CAL);
				jdTry = NewMoonJD;
				NewMoonVisibleByEye = false;
                NewMoonVisibleByTelescope = false;
				for (int days = 0; days < 6; days++) {
					irm1 = 0.0;
					irm2 = 0.0;
					irm3 = 0.0;
					MaximumFound = false;
					mrise = Sweph.getRiseSetTime (Constants.SE_MOON, jdTry, longitude, latitude, altitude, pressure, temperature, 0, Constants.SE_CALC_RISE);
					mrise = Math.Floor (mrise * 24.0 * 60.0 + 0.5) / 24.0 / 60.0;
					if (FirstDayTry) {
						altazM = Sweph.getAltAz (Constants.SE_MOON, jdTry, longitude, latitude, altitude, pressure, temperature);
						if (altazM [2] > 0.5)
							j1 = jdTry;
						else {
							jdTry = mrise + 0.01;
							j1 = mrise;
						}
						FirstDayTry = false;
					} else {
						j1 = mrise;
					}
					Myrevjul (mrise, Constants.SE_GREG_CAL, out yy2, out mm2, out dd2, out hh2);

					sset = Sweph.getRiseSetTime (Constants.SE_SUN, jdTry, longitude, latitude, altitude, pressure, temperature, 0, Constants.SE_CALC_SET);
					Myrevjul (sset, Constants.SE_GREG_CAL, out yy1, out mm1, out dd1, out hh1);


					mset = Sweph.getRiseSetTime (Constants.SE_MOON, jdTry, longitude, latitude, altitude, pressure, temperature, 0, Constants.SE_CALC_SET);
					Myrevjul (mset, Constants.SE_GREG_CAL, out yy2, out mm2, out dd2, out hh2);
					//q = (ArcVg - (11.8371 - 6.3226 * W + 0.7319 * W * W - 0.1018 * W * W * W)) / 10.0;
					//j1 = mrise;
					j2 = Math.Min (mset, sset);
					j2 = sset - Math.Abs (mset - sset);   // Rostami
					jdTry = Math.Max (mset, sset) + .1;
					//
					// Best time
					//
					bestTime = (5.0 * sset + 4.0 * mset) / 9.0;
					altazS = Sweph.getAltAz (Constants.SE_SUN, bestTime, longitude, latitude, altitude, pressure, temperature);
					altazM = Sweph.getAltAz (Constants.SE_MOON, bestTime, longitude, latitude, altitude, pressure, temperature);
					MoonDetail = Sweph.getPhenoUt (Constants.SE_MOON, bestTime, iflag);
					W = 60.0 * MoonDetail [1] * MoonDetail [3];
					dh = Math.Abs (altazM [1] - altazS [1]);
					dA = Math.Abs (altazM [0] - altazS [0]);
					if (dA > 180.0)
						dA = 360.0 - dA;
					//
					// Rostami
					//
					irmBestTime = 0.3479 * dA + 1.2749 * dh + 2.4966 * W;
					//irmBestTime = 0.1795 * dA + 1.299 * dh + 7.56 * W;
					//
					// Odeh
					//
					ArcV = dh;
					//ArcVg = Math.Abs(hm - hs);
					V = ArcV - (-0.1018 * W * W * W + 0.7319 * W * W - 6.3226 * W + 7.1651);

					//textBox1.Text += "Best Time : JD = " + bestTime.ToString("#######.##0");
					//textBox1.Text += "  ,  " + dd1.ToString() + "/" + mm1.ToString() + "/" + yy1.ToString() + " , " + String.Format("{0:ahms}", new Sexagesimal(hh1, 600));
					//textBox1.Text += " , V = " + V.ToString("##.###") + " \r\n";
					Myrevjul (sset + tz / 24.0, Constants.SE_GREG_CAL, out yy1, out mm1, out dd1, out hh1);
					strb.Append ("Sun set : JD = ");
					strb.Append (sset.ToString ("#######.##0"));
					strb.Append ("  ,  ");
					strb.Append (dd1.ToString ());
					strb.Append ("/");
					strb.Append (mm1.ToString ());
					strb.Append ("/");
					strb.Append (yy1.ToString ());
					strb.Append (" , ");
					strb.AppendLine (String.Format ("{0:ahms}", new Sexagesimal (hh1, 600)));

					Myrevjul (mset + tz / 24.0, Constants.SE_GREG_CAL, out yy1, out mm1, out dd1, out hh1);
					strb.Append ("Moon set : JD = ");
					strb.Append (mset.ToString ("#######.##0"));
					strb.Append ("  ,  ");
					strb.Append (dd1.ToString ());
					strb.Append ("/");
					strb.Append (mm1.ToString ());
					strb.Append ("/");
					strb.Append (yy1.ToString ());
					strb.Append (" , ");
					strb.AppendLine (String.Format ("{0:ahms}", new Sexagesimal (hh1, 600)));
					
					Myrevjul (bestTime + tz / 24.0, Constants.SE_GREG_CAL, out yy1, out mm1, out dd1, out hh1);
					strb.Append ("Best Time : JD = ");
					strb.Append (bestTime.ToString ("#######.##0"));
					strb.Append ("  ,  ");
					strb.Append (dd1.ToString ());
					strb.Append ("/");
					strb.Append (mm1.ToString ());
					strb.Append ("/");
					strb.Append (yy1.ToString ());
					strb.Append (" , ");
					strb.AppendLine (String.Format ("{0:ahms}", new Sexagesimal (hh1, 600)));
					strb.Append ("V (Odeh) = ");
					strb.AppendLine (V.ToString ("##.###"));
					strb.Append("  ARCV = ");
					strb.Append(ArcV.ToString("##.#####"));
					strb.Append("   W'=   ");
					strb.AppendLine(W.ToString("##.#####"));
					strb.Append ("irm (Rostami) = ");
					strb.AppendLine (irmBestTime.ToString ("##.###"));
				
				
					for (double jdf = j1; jdf < j2; jdf += djd) {
						altazS = Sweph.getAltAz (Constants.SE_SUN, jdf, longitude, latitude, altitude, pressure, temperature);
						altazM = Sweph.getAltAz (Constants.SE_MOON, jdf, longitude, latitude, altitude, pressure, temperature);
						dh = Math.Abs (altazM [1] - altazS [1]);
						dA = Math.Abs (altazM [0] - altazS [0]);
						if (dA > 180.0)
							dA = 360.0 - dA;
						//if(dA<-180)
						MoonDetail = Sweph.getPhenoUt (Constants.SE_MOON, jdf, iflag);
						W = 60.0 * MoonDetail [1] * MoonDetail [3];
						irm = 27.13 * W + 0.139 * altazM [1] - 0.49 * dh; //day
						Myrevjul (jdf + tz / 24.0, Constants.SE_GREG_CAL, out yy1, out mm1, out dd1, out hh1);
						//textBox1.Text += "JD = " + jdf.ToString("#######.##0");
						//textBox1.Text += "  ,  " + dd1.ToString() + "/" + mm1.ToString() + "/" + yy1.ToString() + " , " + String.Format("{0:ahms}", new Sexagesimal(hh1, 600));
						//textBox1.Text += " , hm = " + altazM[1].ToString("###.####") + " , w = " + W.ToString("##.###") + " , ";
						//textBox1.Text += " , ir_m = " + irm.ToString("0#.##0");
					
					
						//if
						irm2 = irm;
					
						if (irm1 < 10.0 && irm2 >= 10.0) {
							strb.Append ("JD = ");
							strb.Append (jdf.ToString ("#######.##0"));
							strb.Append ("  ,  ");
							strb.Append (dd1.ToString ());
							strb.Append ("/");
							strb.Append (mm1.ToString ());
							strb.Append ("/");
							strb.Append (yy1.ToString ());
							strb.Append (" , ");
							strb.Append (String.Format ("{0:ahms}", new Sexagesimal (hh1, 600)));
							strb.Append (" , hm = ");
							strb.Append (altazM [1].ToString ("###.##"));
							strb.Append (" , w = ");
							strb.Append (W.ToString ("##.###"));
							strb.Append (" , ir_m = ");
							strb.Append (irm.ToString ("##.##0"));
							strb.AppendLine (" Telescope starts");
						
						}
						if (irm1 >= 10.0 && irm2 < 10.0) {
							strb.Append ("JD = ");
							strb.Append (jdf.ToString ("#######.##0"));
							strb.Append ("  ,  ");
							strb.Append (dd1.ToString ());
							strb.Append ("/");
							strb.Append (mm1.ToString ());
							strb.Append ("/");
							strb.Append (yy1.ToString ());
							strb.Append (" , ");
							strb.Append (String.Format ("{0:ahms}", new Sexagesimal (hh1, 600)));
							strb.Append (" , hm = ");
							strb.Append (altazM [1].ToString ("###.##"));
							strb.Append (" , w = ");
							strb.Append (W.ToString ("##.###"));
							strb.Append (" , ");
							strb.Append (" , ir_m = ");
							strb.Append (irm.ToString ("##.##0"));
							strb.AppendLine (" Telescope ends");
						}
						//
						if (irm1 < 30.0 && irm2 >= 30.0) {
							strb.Append ("JD = ");
							strb.Append (jdf.ToString ("#######.##0"));
							strb.Append ("  ,  ");
							strb.Append (dd1.ToString ());
							strb.Append ("/");
							strb.Append (mm1.ToString ());
							strb.Append ("/");
							strb.Append (yy1.ToString ());
							strb.Append (" , ");
							strb.Append (String.Format ("{0:ahms}", new Sexagesimal (hh1, 600)));
							strb.Append (" , hm = ");
							strb.Append (altazM [1].ToString ("###.##"));
							strb.Append (" , w = ");
							strb.Append (W.ToString ("##.###"));
							strb.Append (" , ");
							strb.Append (" , ir_m = ");
							strb.Append (irm.ToString ("##.##0"));
							strb.AppendLine (" Eye starts");
						}
						if (irm1 >= 30.0 && irm2 < 30.0) {
							strb.Append ("JD = ");
							strb.Append (jdf.ToString ("#######.##0"));
							strb.Append ("  ,  ");
							strb.Append (dd1.ToString ());
							strb.Append ("/");
							strb.Append (mm1.ToString ());
							strb.Append ("/");
							strb.Append (yy1.ToString ());
							strb.Append (" , ");
							strb.Append (String.Format ("{0:ahms}", new Sexagesimal (hh1, 600)));
							strb.Append (" , hm = ");
							strb.Append (altazM [1].ToString ("###.##"));
							strb.Append (" , w = ");
							strb.Append (W.ToString ("##.###"));
							strb.Append (" , ");
							strb.Append (" , ir_m = ");
							strb.Append (irm.ToString ("##.##0"));
							strb.AppendLine (" Eye ends");
						}
						if (! MaximumFound) {
							if (irm2 <= irm1) {
								MaximumFound = true;
								strb.Append ("JD = ");
								strb.Append (jdf.ToString ("#######.##0"));
								strb.Append ("  ,  ");
								strb.Append (dd1.ToString ());
								strb.Append ("/");
								strb.Append (mm1.ToString ());
								strb.Append ("/");
								strb.Append (yy1.ToString ());
								strb.Append (" , ");
								strb.Append (String.Format ("{0:ahms}", new Sexagesimal (hh1, 600)));
								strb.Append (" , hm = ");
								strb.Append (altazM [1].ToString ("###.##"));
								//strb.Append(" , dh = ");
								//strb.Append(dh.ToString("##.##"));
								strb.Append (" , w = ");
								strb.Append (W.ToString ("##.###"));
								strb.Append (" , ");
								strb.Append (" , ir_m = ");
								strb.Append (irm.ToString ("##.##0"));
								strb.AppendLine (" Maximum irm");
								irmMax = irm1;
								JDMax = jdf;
							}
						}
						irm1 = irm;
					}
					if (! MaximumFound) {
						MaximumFound = true;
						strb.Append ("JD = ");
						strb.Append (j2.ToString ("#######.##0"));
						strb.Append ("  ,  ");
						strb.Append (dd1.ToString ());
						strb.Append ("/");
						strb.Append (mm1.ToString ());
						strb.Append ("/");
						strb.Append (yy1.ToString ());
						strb.Append (" , ");
						strb.Append (String.Format ("{0:ahms}", new Sexagesimal (hh1, 600)));
						strb.Append (" , hm = ");
						strb.Append (altazM [1].ToString ("###.##"));
						//strb.Append(" , dh = ");
						//strb.Append(dh.ToString("##.##"));
						strb.Append (" , w = ");
						strb.Append (W.ToString ("##.###"));
						strb.Append (" , ");
						strb.Append (" , ir_m = ");
						strb.Append (irm.ToString ("##.##0"));
						strb.AppendLine (" Maximum irm");
						irmMax = irm1;
						JDMax = j2;
					}
					strb.AppendLine ();
					//
					//  Ir_m in Daytime
					//
					if (irmMax < 10) {
						ViewDeviceDay = "Not visible";
						strb.AppendLine ("Moon is not visible in daytime (rostami)");
					} else {
						if (irmMax < 12)
							ViewDeviceDay = "10\" telescope";
						else if (irm1 < 14)
							ViewDeviceDay = "8\" telescope";
						else if (irmMax < 16)
							ViewDeviceDay = "40X150";
						else if (irmMax < 18)
							ViewDeviceDay = "20X120";
						else if (irmMax < 20)
							ViewDeviceDay = "15X80";
						else if (irmMax < 21)
							ViewDeviceDay = "15X70";
						else if (irmMax < 22)
							ViewDeviceDay = "20X60";
						else if (irmMax < 30)
							ViewDeviceDay = "10X50";
						else
							ViewDeviceDay = "Naked eye";
						strb.AppendLine ("Moon is visible by " + ViewDeviceDay + " in daytime");
					}
					//
					//  Evening
					//

					if (irmBestTime < 10) {
						ViewDeviceEvening = "Not visible";
						strb.AppendLine ("Moon is not visible at the evening (rostami)");
					} else {
						if (irmBestTime < 10.25)
							ViewDeviceEvening = "8\" telescope and higher";
						else if (irmBestTime < 10.45)
							ViewDeviceEvening = "40X150";
						else if (irmBestTime < 10.7)
							ViewDeviceEvening = "20X120";
						else if (irmBestTime < 11.0)
							ViewDeviceEvening = "20X100";
						else if (irmBestTime < 11.60)
							ViewDeviceEvening = "15X80";
						else if (irmBestTime < 12.1)
							ViewDeviceEvening = "15X70";
						else if (irmBestTime < 12.65)
							ViewDeviceEvening = "20X60";
						else if (irmBestTime < 13.35)
							ViewDeviceEvening = "10X50";
						else
							ViewDeviceEvening = "Naked eye";
						strb.AppendLine ("Moon is visible by " + ViewDeviceEvening + " at the evening (rostami)");
					}
					if (irmBestTime >= 13.5)
						NewMoonVisibleByEye = true;
                    if (irmBestTime >= 10.0 || irmMax >= 12.0)
                        NewMoonVisibleByTelescope = true;
					//
					// Odeh
					//
					if (V >= 5.65)
						OdehVisibility = "Odeh: Visible by naked eye";
					else if (V >= 2.0)
						OdehVisibility = "Odeh: Visible by optical aid, could be seen by naked eye";
					else if (V >= -0.96)
						OdehVisibility = "Odeh: Visible by optical aid only";
					else
						OdehVisibility = "Odeh: Not visible";
					strb.AppendLine (OdehVisibility);
                    if (NewMoonVisibleByTelescope)
                    {
                        jdp = Sweph.getJD(yy1, mm1, dd1, 0.0, Constants.SE_GREG_CAL) - 2386436.5;
                        tb2.Append(jdp.ToString());
                        tb2.Append(", ");
                        cntM++;
                        if (cntM > 12)
                        {
                            tb2.AppendLine(" // " + cntY.ToString());
                            textBox2.Text = tb2.ToString();
                            textBox2.Refresh();
                            cntY++;
                            cntM = 1;
                        }

                        break;
                    }
					strb.AppendLine ("----------------------------------------------------------");
				
				}
				strb.AppendLine();
				strb.AppendLine ("<<================================================================>>");
				strb.AppendLine();
				textBox1.Text = strb.ToString ();
				textBox1.Refresh ();
			}
			textBox1.Text = strb.ToString ();
            textBox2.Text = tb2.ToString();
            textBox1.Refresh();
            textBox2.Refresh();
			
		}
	}
	//textBox3.Text += "     Sun set  = " + dd1.ToString() + "/" + mm1.ToString() + "/" + yy1.ToString() + "  ,  " + String.Format("{0:ahms}", new MySofa.Sexagesimal(hh1, 3.6e3)) + "\r\n";
	// Moon

}

