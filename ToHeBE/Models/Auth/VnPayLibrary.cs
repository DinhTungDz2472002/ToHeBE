using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Web;

public class VnPayLibrary
{
	private readonly SortedDictionary<string, string> _requestData = new SortedDictionary<string, string>();
	public const string VERSION = "2.1.0";

	public void AddRequestData(string key, string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			_requestData.Add(key, value);
		}
	}

	public SortedDictionary<string, string> GetRequestData()
	{
		return _requestData;
	}

	public string CreateRequestUrl(string baseUrl, string hashSecret)
	{
		if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(hashSecret))
			throw new ArgumentNullException("baseUrl or hashSecret cannot be null or empty.");

		StringBuilder data = new StringBuilder();
		foreach (var kvp in _requestData)
		{
			if (!string.IsNullOrEmpty(kvp.Value))
			{
				data.Append(HttpUtility.UrlEncode(kvp.Key) + "=" + HttpUtility.UrlEncode(kvp.Value) + "&");
			}
		}

		string queryString = data.ToString().TrimEnd('&');
		string vnp_SecureHash = HmacSHA512(hashSecret, queryString);
		return baseUrl + "?" + queryString + "&vnp_SecureHash=" + vnp_SecureHash;
	}

	public string HmacSHA512(string key, string inputData)
	{
		if (string.IsNullOrEmpty(key))
			throw new ArgumentNullException(nameof(key));
		if (inputData == null)
			throw new ArgumentNullException(nameof(inputData));

		var hash = new StringBuilder();
		byte[] keyBytes = Encoding.UTF8.GetBytes(key);
		byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
		using (var hmac = new HMACSHA512(keyBytes))
		{
			byte[] hashValue = hmac.ComputeHash(inputBytes);
			foreach (var b in hashValue)
			{
				hash.Append(b.ToString("x2"));
			}
		}
		return hash.ToString();
	}
}