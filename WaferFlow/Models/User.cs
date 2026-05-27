using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaferFlow.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Login { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Patronymic { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string AvatarPath { get; set; }// Путь к аватарке
        public byte[] AvatarData { get; set; }// Данные аватарки
        public string AvatarContentType { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Role { get; set; }
        public int? ShiftId { get; set; }// ID смены (может быть null)
        public DateTime? LastLogin { get; set; }// Время последнего входа
        public string DisplayInfo { get; set; }

        public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();


        public string Initials
        {
            get
            {
                if (!string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
                    return $"{FirstName[0]}{LastName[0]}".ToUpper();
                return "??";
            }
        }

        public string ShortName
        {
            get
            {
                string patronymicInitial = string.IsNullOrEmpty(Patronymic) ? "" : $"{Patronymic[0]}.";
                return $"{LastName} {FirstName[0]}. {patronymicInitial}".Trim();
            }
        }
    }
}