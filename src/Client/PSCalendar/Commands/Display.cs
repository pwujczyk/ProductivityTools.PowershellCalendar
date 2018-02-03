﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PSCalendarContract.Dto;
using PSCalendarTools;

namespace PSCalendar.Commands
{
    class Display : BaseCommand
    {
        private static string Spacer = "".PadLeft(4);

        protected override bool Condition => true;

        public Display(PSCalendarCmdlet cmdl) : base(cmdl) { }

        protected override void Invoke()
        {

            ShowSelectedMonth(this.Cmdlet.ShowMonth.HasValue ? DateTime.Now.AddMonths(this.Cmdlet.ShowMonth.Value) : DateTime.Now);
        }

        private void ShowSelectedMonth(DateTime date)
        {
            ShowCalendarRange(date.GetFirstMonthDay(), date.GetLastMonthDay());
        }

        private void ShowCalendarRange(DateTime start, DateTime end)
        {
            var eventsList = GetEvents(start, end);
            Stack<Event> eventsStack = new Stack<Event>(eventsList.OrderByDescending(x => x.Date));
            ShowMonthTitle(start);
            FillEmptyValues(start);
            DateTime lastItem = DateTime.Now;
            foreach (var item in EachDay(start, end))
            {
                WriteDayNumber(eventsList, item);
                if (item.DayOfWeek == DayOfWeek.Sunday)
                {
                    Console.Write("\t");
                    WriteEvent(eventsStack);
                    Console.Write(Environment.NewLine);
                }
                lastItem = item;
            }


            if (eventsStack.Count > 0)
            {
                for (int i = lastItem.DayOfWeek == DayOfWeek.Sunday ? 0 : (int)lastItem.DayOfWeek; i <= 7; i++)
                {
                    Console.Write("".PadLeft(4));
                }

                WriteEvent(eventsStack);
            }

            Console.Write(Environment.NewLine);

            for (int i = eventsStack.Count; i > 0; i--)
            {
                Console.Write("\t"); Console.Write("\t"); Console.Write("\t"); Console.Write("\t");
                WriteEvent(eventsStack);
                Console.Write(Environment.NewLine);
            }
            Console.WriteLine("");
        }


        public IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
                yield return day;
        }

        private List<Event> GetEvents(DateTime start, DateTime end)
        {

            //todo:close client
            return Client.GetEvents(start, end).ToList();
        }

        private void ShowMonthTitle(DateTime start)
        {
            Console.WriteLine(string.Format("[{0}] ", start.Month.ToString().PadLeft(2, '0')) + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(start.Month));
        }


        private void FillEmptyValues(DateTime start)
        {
            int delta = start.DayOfWeek - DayOfWeek.Sunday;
            if (delta == 0)
            {
                delta = 7;
            }
            for (int i = 0; i < delta - 1; i++)
            {
                Console.Write(Spacer);
            }
        }

        private static void WriteDayNumber(List<Event> eventsList, DateTime item)
        {
            var @event = eventsList.Where(x => x.Date.ToShortDateString() == item.ToShortDateString()).ToList();
            if (@event.Count > 0)
            {
                if (@event.Count == 1)
                {
                    Console.ForegroundColor = CommonExtensions.ParseEnum<System.ConsoleColor>(@event[0].Color.ToString());
                }
                else
                {
                    Console.ForegroundColor = System.ConsoleColor.Red;
                }
            }
            if (item.ToShortDateString() == DateTime.Now.ToShortDateString())
            {
                Console.Write(string.Concat(".", item.Day.ToString()).PadLeft(4));
            }
            else
            {
                Console.Write(item.Day.ToString().PadLeft(4));
            }
            Console.ResetColor();
        }



        private void WriteEvent(Stack<Event> eventsStack)
        {
            if (eventsStack.Count > 0)
            {
                Event e = eventsStack.Pop();
                Console.ForegroundColor = CommonExtensions.ParseEnum<System.ConsoleColor>(e.Color.ToString());
                Console.Write(string.Format("{0}. {1}    {2}", e.EventsId, e.Date, e.Name));
                Console.ResetColor();

            }
        }

    }
}