using System;

namespace LegacyApp
{
	public enum ClientType { VeryImportantClient, ImportantClient }

	//This Class is missing a good programming principle. Code to an interface not implementation
	//Difficult to write Unit tests.
	public class UserService
	{
		public bool AddUser(string firstName, string surname, string email, DateTime dateOfBirth, int clientId)
		{
			try
			{
				if (!ValidateUserDetail(firstName, surname, email, dateOfBirth))
					return false;

				//new is glue Dependency Injection (IOC container) would be handy

				var clientRepository = new ClientRepository();
				var client = clientRepository.GetById(clientId);
				var userCreditDetails = GetUserCreditLimit(client, firstName, surname, dateOfBirth);

				var user = new User
				{
					Client = client,
					DateOfBirth = dateOfBirth,
					EmailAddress = email,
					Firstname = firstName,
					Surname = surname,
					CreditLimit = userCreditDetails.CreditLimit,
					HasCreditLimit = userCreditDetails.HasCreditLimit
				};

				if (user.HasCreditLimit && user.CreditLimit < 500)
				{
					return false;
				}

				UserDataAccess.AddUser(user);

				return true;
			}
			catch (Exception ex)
			{
				throw new Exception($"The below error occurred while adding a new user {ex.Message}");
			}
		}

		public UserCreditDetails GetUserCreditLimit(Client client, string firstname, string surname, DateTime dateOfBirth) => client.Name switch 
		{
			nameof(ClientType.ImportantClient) => GetImportantClientCreditDetails(firstname, surname, dateOfBirth),
			nameof(ClientType.VeryImportantClient) =>  new UserCreditDetails(),
			 _ => GetDefaultClientCreditDetails(firstname, surname, dateOfBirth)
		};

		public UserCreditDetails GetImportantClientCreditDetails(string firstname, string surname, DateTime dateOfBirth)
		{
			UserCreditDetails userCreditDetails = new UserCreditDetails();
			GetCreditLimit(firstname, surname, dateOfBirth, (creditLimit) => {
				creditLimit = creditLimit * 2;

				userCreditDetails = new UserCreditDetails
				{
					CreditLimit = creditLimit,
					HasCreditLimit = true
				};
			});
			return userCreditDetails;
		}

		public UserCreditDetails GetDefaultClientCreditDetails(string firstname, string surname, DateTime dateOfBirth)
		{
			UserCreditDetails userCreditDetails = new UserCreditDetails();
			GetCreditLimit(firstname, surname, dateOfBirth, (creditLimit) => {

				userCreditDetails = new UserCreditDetails
				{
					CreditLimit = creditLimit,
					HasCreditLimit = true
				};
			});
			return userCreditDetails;
		}

		public void GetCreditLimit(string firstname, string surname, DateTime dateOfBirth, Action<int> creditLimitAction)
		{
			using (var userCreditService = new UserCreditServiceClient())
			{
				var creditLimit = userCreditService.GetCreditLimit(firstname, surname, dateOfBirth);
				creditLimitAction(creditLimit);
			}
		}


		#region Can be moved to a utility or helper class for reuseability

		public bool ValidateUserDetail(string firstName, string surName, string email, DateTime dateOfBirth)
		{
			var isFullNameValid = ValidateFullName(firstName, surName);
			var isValidDOB = ValidateDOB(dateOfBirth);
			var isValidEmail = ValidateEmail(email);

			return (isFullNameValid && isValidDOB && isValidEmail);
		}

		public bool ValidateFullName(string firstName, string surName)
			=> string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(surName) ? false : true;

		public bool ValidateEmail(string email)
			=> (email.Contains("@") && email.Contains("."));

		public bool ValidateDOB(DateTime dateOfBirth)
		{
			var now = DateTime.Now;
			int age = now.Year - dateOfBirth.Year;

			if (now.Month < dateOfBirth.Month ||
			   (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

			return age < 21 ? false : true;
		}
		#endregion
	}
}
