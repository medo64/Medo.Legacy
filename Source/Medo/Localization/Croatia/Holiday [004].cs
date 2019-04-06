/* Josip Medved <jmedved@jmedved.com> * www.medo64.com * MIT License */

//2012-10-30: Added enumerations.
//2008-11-05: Easter is also holiday.
//2008-04-11: Cleaned code to match FxCop 1.36 beta 2.
//2008-02-15: New version.


using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Medo.Localization.Croatia {

    /// <summary>
    /// Detecting Croatian hollidays.
    /// </summary>
    public static class Holiday {

        /// <summary>
        /// Returns true if given date is public holiday.
        /// Valid dates are defined from 1991-03-25. If lower date is given, exception will be thrown.
        /// </summary>
        /// <param name="date">Date to check.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Date must be larger or equal to 1991-03-25.</exception>
        public static bool IsHoliday(DateTime date) {
            foreach (var holiday in Helper.GetHolidays(date, false)) {
                if (holiday.Date == date) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Returns list of all holidays in given year.
        /// </summary>
        /// <param name="date">Date to use as basis for check.</param>
        public static IEnumerable<HolidayEntry> RetrieveAll(DateTime date) {
            if (date < new DateTime(1991, 03, 25)) { throw new ArgumentOutOfRangeException("date", Resources.ExceptionDateMustBeLargerOrEqualTo19910325); }

            foreach (var holiday in Helper.GetHolidays(date, true)) {
                yield return new HolidayEntry(holiday.Date, holiday.Title);
            }
        }


        /// <summary>
        /// Returns easter date for given year.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">Year is out of range (1753-4000).</exception>
        public static DateTime GetEasterDate(int year) {
            if ((year < 1753) || (year > 4000)) { throw new System.ArgumentOutOfRangeException("year", Resources.ExceptionYearIsOutOfRange); }

            int tmpMonth, tmpDay;

            int a, b, c, d, e, f, g, h, i, j, k, l, m;

            a = year % 19;
            b = year / 100;
            c = year % 100;
            d = b / 4;
            e = b % 4;
            f = (b + 8) / 25;
            g = (b - f + 1) / 3;
            h = (19 * a + b - d - g + 15) % 30;
            i = c / 4;
            j = c % 4;
            k = (32 + 2 * e + 2 * i - h - j) % 7;
            l = (a + 11 * h + 22 * k) / 451;
            m = (h + k - 7 * l + 114) % 31;

            tmpMonth = (h + k - 7 * l + 114) / 31;
            tmpDay = m + 1;

            return new DateTime(year, tmpMonth, tmpDay);
        }


        private static class Helper {

            internal static IList<HolidayEntry> GetHolidays(DateTime date, bool sort) {
                List<HolidayEntry> list = null;
                if (date >= new DateTime(2002, 02, 16)) {
                    list = Helper.GetHolidaysAfter20020216(date.Year);
                } else if (date >= new DateTime(2001, 11, 15)) {
                    list = Helper.GetHolidaysAfter20011115(date.Year);
                } else if (date >= new DateTime(1996, 05, 08)) {
                    list = Helper.GetHolidaysAfter19960508(date.Year);
                } else if (date >= new DateTime(1991, 03, 25)) {
                    list = Helper.GetHolidaysAfter19910325(date.Year);
                }
                if (sort && (list != null)) {
                    list.Sort(delegate(HolidayEntry h1, HolidayEntry h2) { return DateTime.Compare(h1.Date, h2.Date); });
                }
                return list;
            }

            internal static List<HolidayEntry> GetHolidaysAfter20020216(int year) { //NN 13/02
                var list = new List<HolidayEntry> {
                    new HolidayEntry(new DateTime(year, 1, 1), "Nova godina"),
                    new HolidayEntry(new DateTime(year, 1, 6), "Bogojavljanje ili Sveta tri kralja"),
                    new HolidayEntry(new DateTime(year, 5, 1), "Praznik rada"),
                    new HolidayEntry(new DateTime(year, 6, 22), "Dan antifašističke borbe"),
                    new HolidayEntry(new DateTime(year, 6, 25), "Dan državnosti"),
                    new HolidayEntry(new DateTime(year, 8, 5), "Dan pobjede i domovinske zahvalnosti"),
                    new HolidayEntry(new DateTime(year, 8, 15), "Velika Gospa"),
                    new HolidayEntry(new DateTime(year, 10, 8), "Dan neovisnosti"),
                    new HolidayEntry(new DateTime(year, 11, 1), "Svi sveti"),
                    new HolidayEntry(new DateTime(year, 12, 25), "Božićni blagdani"),
                    new HolidayEntry(new DateTime(year, 12, 26), "Božićni blagdani")
                };

                var e = GetEasterDate(year);
                list.Add(new HolidayEntry(e, "Uskrs")); //nije u NN

                var e1 = e.AddDays(1);
                list.Add(new HolidayEntry(e1, "Uskrsni ponedjeljak – drugi dan Uskrsa"));

                var e60 = e.AddDays(60);
                list.Add(new HolidayEntry(e60, "Tijelovo"));

                return list;
            }

            internal static List<HolidayEntry> GetHolidaysAfter20011115(int year) { //NN 96/01
                var list = new List<HolidayEntry> {
                    new HolidayEntry(new DateTime(year, 1, 1), "Nova godina"),
                    new HolidayEntry(new DateTime(year, 5, 1), "Praznik rada"),
                    new HolidayEntry(new DateTime(year, 6, 22), "Dan antifašističke borbe"),
                    new HolidayEntry(new DateTime(year, 6, 25), "Dan državnosti"),
                    new HolidayEntry(new DateTime(year, 8, 5), "Dan pobjede i domovinske zahvalnosti"),
                    new HolidayEntry(new DateTime(year, 8, 15), "Velika Gospa"),
                    new HolidayEntry(new DateTime(year, 10, 8), "Dan neovisnosti"),
                    new HolidayEntry(new DateTime(year, 11, 1), "Svi sveti"),
                    new HolidayEntry(new DateTime(year, 12, 25), "Božićni blagdani"),
                    new HolidayEntry(new DateTime(year, 12, 26), "Božićni blagdani")
                };

                var e = GetEasterDate(year);
                list.Add(new HolidayEntry(e, "Uskrs")); //nije u NN

                var e1 = e.AddDays(1);
                list.Add(new HolidayEntry(e1, "Uskrsni ponedjeljak – drugi dan Uskrsa"));

                var e60 = e.AddDays(60);
                list.Add(new HolidayEntry(e60, "Tijelovo"));

                return list;
            }

            internal static List<HolidayEntry> GetHolidaysAfter19960508(int year) { //NN 33/96
                var list = new List<HolidayEntry> {
                    new HolidayEntry(new DateTime(year, 1, 1), "Nova godina"),
                    new HolidayEntry(new DateTime(year, 1, 6), "Bogojavljanje ili Sveta tri kralja"),
                    new HolidayEntry(new DateTime(year, 5, 1), "Praznik rada"),
                    new HolidayEntry(new DateTime(year, 5, 30), "Dan državnosti"),
                    new HolidayEntry(new DateTime(year, 6, 22), "Dan antifašističke borbe"),
                    new HolidayEntry(new DateTime(year, 8, 5), "Dan pobjede i domovinske zahvalnosti"),
                    new HolidayEntry(new DateTime(year, 8, 15), "Velika Gospa"),
                    new HolidayEntry(new DateTime(year, 11, 1), "Svi sveti"),
                    new HolidayEntry(new DateTime(year, 12, 25), "Božićni blagdani"),
                    new HolidayEntry(new DateTime(year, 12, 26), "Božićni blagdani")
                };

                var e = GetEasterDate(year);
                list.Add(new HolidayEntry(e, "Uskrs")); //nije u NN

                var e1 = e.AddDays(1);
                list.Add(new HolidayEntry(e1, "Uskrsni ponedjeljak – drugi dan Uskrsa"));

                return list;
            }

            internal static List<HolidayEntry> GetHolidaysAfter19910325(int year) { //NN 14/91
                var list = new List<HolidayEntry> {
                    new HolidayEntry(new DateTime(year, 1, 1), "Nova godina"),
                    new HolidayEntry(new DateTime(year, 1, 6), "(neradni dan)"),
                    new HolidayEntry(new DateTime(year, 1, 7), "(neradni dan)"),
                    new HolidayEntry(new DateTime(year, 5, 1), "Praznik rada"),
                    new HolidayEntry(new DateTime(year, 5, 30), "Dan državnosti"),
                    new HolidayEntry(new DateTime(year, 6, 22), "Dan antifašističke borbe"),
                    new HolidayEntry(new DateTime(year, 8, 15), "Velika Gospa"),
                    new HolidayEntry(new DateTime(year, 11, 1), "Svi sveti"),
                    new HolidayEntry(new DateTime(year, 12, 25), "Božićni blagdani"),
                    new HolidayEntry(new DateTime(year, 12, 26), "Božićni blagdani")
                };

                var e = GetEasterDate(year);
                list.Add(new HolidayEntry(e, "Uskrs")); //nije u NN

                var e1 = e.AddDays(1);
                list.Add(new HolidayEntry(e1, "Uskrsni ponedjeljak – drugi dan Uskrsa"));

                return list;
            }

        }


        private static class Resources {

            internal static string ExceptionDateMustBeLargerOrEqualTo19910325 { get { return "Date must be larger or equal to 1991-03-25."; } }

            internal static string ExceptionYearIsOutOfRange { get { return "Year is out of range (1753-4000)."; } }

        }

    }


    /// <summary>
    /// Holiday entry.
    /// </summary>
    [DebuggerDisplay("{Date} {Title}")]
    public class HolidayEntry {

        internal HolidayEntry(DateTime date, string title) {
            Date = date;
            Title = title;
        }

        /// <summary>
        /// Gets holiday date.
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// Gets title of holiday.
        /// </summary>
        public string Title { get; private set; }

    }

}
