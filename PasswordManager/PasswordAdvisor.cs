using System.Text.RegularExpressions;

namespace PasswordManager
{

	internal enum PasswordScore
	{
		Blank = 0,
		VeryWeak = 1,
		Weak = 2,
		Medium = 3,
		Strong = 4,
		VeryStrong = 5
	}

	internal class PasswordAdvisor
	{
		public static PasswordScore CheckStrength(string password)
		{
			var score = 0;

			if (password.Length < 1)
				return PasswordScore.Blank;
			if (password.Length < 4)
				return PasswordScore.VeryWeak;

			if (password.Length >= 8)
				score++;
			if (password.Length >= 12)
				score++;
			if (Regex.IsMatch(password, @"\d+"))
				score++;
			if (Regex.IsMatch(password, @"[a-z]") && Regex.IsMatch(password, @"[A-Z]"))
				score++;
			if (Regex.IsMatch(password, @".[!,@,#,$,%,^,&,*,?,_,~,-,£,(,)]"))
				score++;

			return (PasswordScore)score;
		}
	}
}