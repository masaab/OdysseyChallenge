namespace LegacyApp.UnitTests
{
    public class UserServiceUnitTest
    {
        [Theory]
        [InlineData("Peter","Parker","peterParker@gmail.com","Aug 8, 1962")]
        public void ShouldHaveValidUserDetails(string firstName, string lastName, string email, string DOB)
        {
          //Arrange
          UserService userService = new UserService();

            //Act
          var isValidUserDetail = userService.ValidateUserDetail(firstName, lastName, email, DateTime.Parse(DOB));

            //Assert
          Assert.True(isValidUserDetail);
        }


    }
}