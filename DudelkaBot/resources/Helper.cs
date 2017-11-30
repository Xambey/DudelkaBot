using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DudelkaBot.resources
{
    public static class Helper
    {
        /// <summary>
        /// Возвращает слова в падеже, зависимом от заданного числа 
        /// </summary>
        /// <param name="number">Число от которого зависит выбранное слово</param>
        /// <param name="nominativ">Именительный падеж слова. Например "день"</param>
        /// <param name="genetiv">Родительный падеж слова. Например "дня"</param>
        /// <param name="plural">Множественное число слова. Например "дней"</param>
        /// <returns></returns>
        public static string GetDeclension(int number, string nominativ, string genetiv, string plural)
        {
            number = number % 100;
            if (number >= 11 && number <= 19)
            {
                return plural;
            }

            var i = number % 10;
            switch (i)
            {
                case 1:
                    return nominativ;
                case 2:
                case 3:
                case 4:
                    return genetiv;
                default:
                    return plural;
            }

        }
        public static TSource ElementAtOrDefault<TSource>(this IQueryable<TSource> source, int index, bool unused)
        {
            if (index < 0 || index >= source.Count())
                return default(TSource);
            return source.ElementAt(index);
        }

        public static TSource ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index, bool unused)
        {
            int i = 0;
            foreach (var item in source)
            {
                if (i == index)
                    return item;
                i++;
            }
            return default(TSource);
        }
    }
}
