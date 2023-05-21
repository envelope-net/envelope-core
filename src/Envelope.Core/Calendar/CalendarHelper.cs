using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Envelope.Calendar;

public static class CalendarHelper
{
	private const int DaysPerWeekCount = 7;

	private const long TicksPerMillisecond = 10000;
	private const long TicksPerSecond = TicksPerMillisecond * 1000;
	private const long TicksPerMinute = TicksPerSecond * 60;
	private const long TicksPerHour = TicksPerMinute * 60;
	private const long TicksPerDay = TicksPerHour * 24;

	// Number of days in a non-leap year
	private const int DaysPerYear = 365;
	// Number of days in 4 years
	private const int DaysPer4Years = DaysPerYear * 4 + 1;       // 1461
																 // Number of days in 100 years
	private const int DaysPer100Years = DaysPer4Years * 25 - 1;  // 36524
																 // Number of days in 400 years
	private const int DaysPer400Years = DaysPer100Years * 4 + 1; // 146097

	private static readonly Lazy<ConcurrentDictionary<int, List<DateTime>>> _slovakRestDays =
		new(() => new ConcurrentDictionary<int, List<DateTime>>());

	private static readonly Lazy<DateComparer> _dateComparer =
		new(() => new DateComparer());

	public static DateTime GetGregorianEasterSunday(int year)
	{
		int a = year % 19;
		int b = year % 4;
		int c = year % 7;
		int m = 24;
		int n = 5;
		int d = ((19 * a) + m) % 30;
		int e = (n + (2 * b) + (4 * c) + (6 * d)) % 7;
		int march = 22 + d + e;
		int april = d + e - 9;

		return (march <= 31)
			? new DateTime(year, 3, march)
			: new DateTime(year, 4, april);
	}

	public static List<DateTime> GetSlovakRestDays(int year)
		=> _slovakRestDays.Value.GetOrAdd(year, y =>
		{
			var result = new List<DateTime>()
			{
				new DateTime(year, 1, 1),  //Deň vzniku Slovenskej republiky - štátny sviatok
				new DateTime(year, 1, 6),  //Zjavenie Pána (Traja králi) - deň pracovného pokoja
				//Veľký piatok - deň pracovného pokoja
				//Veľkonočný pondelok - deň pracovného pokoja
				new DateTime(year, 5, 1),  //Sviatok práce - deň pracovného pokoja
				new DateTime(year, 5, 8),  //Deň víťazstva nad fašizmom - deň pracovného pokoja
				new DateTime(year, 7, 5),  //Sviatok svätého Cyrila a Metoda - štátny sviatok
				new DateTime(year, 8, 29), //Výročie SNP - štátny sviatok
				new DateTime(year, 9, 1),  //Deň Ústavy Slovenskej republiky - štátny sviatok
				new DateTime(year, 9, 15), //Sedembolestná Panna Mária - deň pracovného pokoja
				new DateTime(year, 11, 1), //Sviatok všetkých svätých - deň pracovného pokoja
				new DateTime(year, 11, 17),//Deň boja za slobodu a demokraciu - štátny sviatok
				new DateTime(year, 12, 24),//Štedrý deň - deň pracovného pokoja
				new DateTime(year, 12, 25),//Prvý sviatok vianočný - deň pracovného pokoja
				new DateTime(year, 12, 26),//Druhý sviatok vianočný - deň pracovného pokoja
		};

			DateTime easterSunday = GetGregorianEasterSunday(year);
			DateTime easterFriday = easterSunday.AddDays(-2); //Veľký piatok - deň pracovného pokoja
			DateTime easterMonday = easterSunday.AddDays(1);  //Veľkonočný pondelok - deň pracovného pokoja
			result.Add(easterFriday);
			result.Add(easterMonday);
			result = result.OrderBy(x => x).ToList();
			return result;
		});

	public static bool IsWeekend(DateTime date)
		=> date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

	public static bool IsWorkday(DateTime date, bool includeWeekends = false)
		=> !GetSlovakRestDays(date.Year).Contains(date, _dateComparer.Value)
			&& (includeWeekends || !IsWeekend(date));

	public static DateTime GetPreviousWorkday(DateTime date, bool includeWeekends = false)
	{
		DateTime result = date.AddDays(-1);
		while (!IsWorkday(result, includeWeekends))
		{
			result = result.AddDays(-1);
		}
		return result;
	}

	public static DateTime GetNextWorkday(DateTime date, bool includeWeekends = false)
	{
		DateTime result = date.AddDays(1);
		while (!IsWorkday(result, includeWeekends))
		{
			result = result.AddDays(1);
		}
		return result;
	}

	public static List<DateTime> GetMonthWorkDays(int month, int year, bool includeWeekends = false)
	{
		var workDays = new List<DateTime>();

		var day = new DateTime(year, month, 1).AddDays(-1);
		var workDay = GetNextWorkday(day, includeWeekends);

		while (workDay.Month == month)
		{
			workDays.Add(workDay);
			workDay = GetNextWorkday(workDay, includeWeekends);
		}

		return workDays;
	}

	public static List<DateTime> GetYearWorkDays(int year, bool includeWeekends = false)
	{
		var workDays = new List<DateTime>();

		var day = new DateTime(year, 1, 1).AddDays(-1);
		var workDay = GetNextWorkday(day, includeWeekends);

		while (workDay.Year == year)
		{
			workDays.Add(workDay);
			workDay = GetNextWorkday(workDay, includeWeekends);
		}

		return workDays;
	}

	private static readonly int[] DaysToMonth365 =
	{
			0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334, 365
		};

	private static readonly int[] DaysToMonth366 =
	{
			0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335, 366
		};

	private static readonly int[] DaysInMonth =
	{
			-1, 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31
		};

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsGreaterThan(int year1, int month1, int day1, int year2, int month2, int day2)
	{
		if (year1 != year2) return year1 > year2;
		if (month1 != month2) return month1 > month2;
		if (day2 != day1) return day1 > day2;
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static long DateTimeToTicks(int year, int month, int day, int hour, int minute, int second)
	{
		int[] days = year % 4 == 0 && (year % 100 != 0 || year % 400 == 0) ? DaysToMonth366 : DaysToMonth365;
		int y = year - 1;
		int n = y * 365 + y / 4 - y / 100 + y / 400 + days[month - 1] + day - 1;
		return n * TicksPerDay + (hour * 3600L + minute * 60L + second) * TicksPerSecond;
	}

	// Returns a given date part of this DateTime. This method is used
	// to compute the year, day-of-year, month, or day part.
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void FillDateTimeParts(long ticks, out int second, out int minute, out int hour,
		out int day, out int month, out int year)
	{
		second = (int)(ticks / TicksPerSecond % 60);
		if (ticks % TicksPerSecond != 0) second++;
		minute = (int)(ticks / TicksPerMinute % 60);
		hour = (int)(ticks / TicksPerHour % 24);

		// n = number of days since 1/1/0001
		int n = (int)(ticks / TicksPerDay);
		// y400 = number of whole 400-year periods since 1/1/0001
		int y400 = n / DaysPer400Years;
		// n = day number within 400-year period
		n -= y400 * DaysPer400Years;
		// y100 = number of whole 100-year periods within 400-year period
		int y100 = n / DaysPer100Years;
		// Last 100-year period has an extra day, so decrement result if 4
		if (y100 == 4) y100 = 3;
		// n = day number within 100-year period
		n -= y100 * DaysPer100Years;
		// y4 = number of whole 4-year periods within 100-year period
		int y4 = n / DaysPer4Years;
		// n = day number within 4-year period
		n -= y4 * DaysPer4Years;
		// y1 = number of whole years within 4-year period
		int y1 = n / DaysPerYear;
		// Last year has an extra day, so decrement result if 4
		if (y1 == 4) y1 = 3;
		// If year was requested, compute and return it
		year = y400 * 400 + y100 * 100 + y4 * 4 + y1 + 1;
		// n = day number within year
		n -= y1 * DaysPerYear;
		// Leap year calculation looks different from IsLeapYear since y1, y4,
		// and y100 are relative to year 1, not year 0
		bool leapYear = y1 == 3 && (y4 != 24 || y100 == 3);
		int[] days = leapYear ? DaysToMonth366 : DaysToMonth365;
		// All months have less than 32 days, so n >> 5 is a good conservative
		// estimate for the month
		month = (n >> 5) + 1;
		// m = 1-based month number

		// day = 1-based day-of-month
		while (n >= days[month]) month++;
		day = n - days[month - 1] + 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static DayOfWeek GetDayOfWeek(int year, int month, int day)
	{
		var isLeapYear = year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);
		int[] days = isLeapYear ? DaysToMonth366 : DaysToMonth365;
		int y = year - 1;
		int n = y * 365 + y / 4 - y / 100 + y / 400 + days[month - 1] + day - 1;
		var ticks = n * TicksPerDay;

		return ((DayOfWeek)((int)(ticks / TicksPerDay + 1) % 7));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int GetDaysInMonth(int year, int month)
	{
		if (month != 2 || year % 4 != 0) return DaysInMonth[month];

		return year % 100 != 0 || year % 400 == 0 ? 29 : 28;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int MoveToNearestWeekDay(int year, int month, int day)
	{
		var dayOfWeek = GetDayOfWeek(year, month, day);

		if (dayOfWeek != DayOfWeek.Saturday && dayOfWeek != DayOfWeek.Sunday) return day;

		return dayOfWeek == DayOfWeek.Sunday
			? day == GetDaysInMonth(year, month)
				? day - 2
				: day + 1
			: day == CronField.DaysOfMonth.First
				? day + 2
				: day - 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsNthDayOfWeek(int day, int n)
	{
		return day - DaysPerWeekCount * n < CronField.DaysOfMonth.First &&
			   day - DaysPerWeekCount * (n - 1) >= CronField.DaysOfMonth.First;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsLastDayOfWeek(int year, int month, int day)
	{
		return day + DaysPerWeekCount > GetDaysInMonth(year, month);
	}

	private class DateComparer : IEqualityComparer<DateTime>
	{
		public bool Equals(DateTime x, DateTime y)
		{
			return x.Date.Equals(y.Date);
		}

		public int GetHashCode(DateTime obj)
		{
			return obj.Date.GetHashCode();
		}
	}
}
