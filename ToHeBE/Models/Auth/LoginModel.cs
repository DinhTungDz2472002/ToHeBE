﻿using System.ComponentModel.DataAnnotations;

namespace ToHeBE.Models.Auth
{
	public class LoginModel
	{
		[Required]
		public string Username { get; set; }

		[Required]
		public string Password { get; set; }
	}
}
