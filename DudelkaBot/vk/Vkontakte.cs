using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using VkNet.Model.RequestParams;

namespace DudelkaBot.vk
{
    public static class Vkontakte
    {
        #region AuntData
        static string OAuth = "b56600be2aef7fab0cbe547f98b46a424f2d00bb1e04bbad2a87a313c6af3527cda9a85fe9b1944bdb9c6";
        #endregion
        private static VkApi vk = new VkApi();
        private static void Authorize()
        {
            while (!vk.IsAuthorized)
            {
                vk.Authorize(OAuth);
            }
        }

        public static long? getUserId(string screenName)
        {
            Authorize();
            var user = vk.Users.Get(screenName);
            if(user != null)
            {
                return user.Id;
            }

            return null;
        }

        public static bool userExist(long id)
        {
            Authorize();
            var user = vk.Users.Get(id);
            return user != null ? true : false;
        }

        public static string getNameTrack(int id)
        {
            Authorize();
            ProfileFields prof = new ProfileFields();
            prof = ProfileFields.Status;
            var user = vk.Users.Get(id, prof, skipAuthorization: true);

            if (user != null)
            {
                var status = vk.Status.Get(id).Audio;

                if (status != null)
                {
                    return status.Artist + " - " + status.Title;
                }
            }

            return string.Empty;
        }
    }
}
