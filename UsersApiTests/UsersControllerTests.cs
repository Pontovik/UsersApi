using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using UsersApi.Controllers;
using UsersApi.Models;

namespace UsersApiTests
{
    public class UsersControllerTests
    {
        private readonly Mock<IUsersApiDbContext> _dbMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _dbMock = new Mock<IUsersApiDbContext>();
            _controller = new UsersController(_dbMock.Object);
            MockMethodSetup();
        }
        [Fact]
        public async Task GetUsers_ReturnAllUsers()
        {
            //Arrange
            var users = new List<User>()
            {
                new User(){Id = 1, Login = "2", Password = "3"},
                new User(){Id = 2,Login = "4", Password = "5"}
            };
            _dbMock.Setup(db => db.Users).ReturnsDbSet(users);
            //Act
            var result = await _controller.GetUsers();

            //Assert
            var model = Assert.IsAssignableFrom<IEnumerable<User>>(result.Value);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task GetUsers_UsersIsNull()
        {
            //Act
            var result = await _controller.GetUsers();

            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetUser_Correct()
        {
            //Arrange
            var user = new User() { Id = 1, Login = "2", Password = "3" };
            _dbMock.Setup(db => db.Users).ReturnsDbSet(new List<User>() { user });
            _dbMock.Setup(db => db.Users.FindAsync(user.Id)).ReturnsAsync(user);
            //Act
            var result = await _controller.GetUser(user.Id);
            //Assert
            var okResult = Assert.IsType<ActionResult<User>>(result);
            var actualUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal(user.Id, actualUser.Id);
            Assert.Equal(user.Login, actualUser.Login);
            Assert.Equal(user.Password, actualUser.Password);
        }

        [Fact]
        public async Task GetUser_UsersIsNull()
        {
            //Arrange
            var user = new User() { Id = 1, Login = "2", Password = "3" };
            //Act
            var result = await _controller.GetUser(user.Id);
            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetUser_UserNotFound()
        {
            //Arrange
            var user = new User() { Id = 1, Login = "2", Password = "3" };
            _dbMock.Setup(db => db.Users).ReturnsDbSet(new List<User>() { user });
            //Act
            var result = await _controller.GetUser(user.Id);
            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetUsersByPage_Correct()
        {
            //Arrange
            var users = new List<User>()
            {
                new User(){Id = 1, Login = "1", Password = "1"},
                new User(){Id = 2,Login = "2", Password = "2"},
                new User(){Id = 3,Login = "3", Password = "3"},
                new User(){Id = 4,Login = "4", Password = "4"},
            };
            int page = 2;
            int pageSize = 2;
            _dbMock.Setup(db => db.Users).ReturnsDbSet(users);
            var expected = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            //Act
            var result = await _controller.GetUsersByPage(page, pageSize);
            //Assert
            Assert.Equal(expected, result.Value);
        }

        [Fact]
        public async Task GetUsersByPage_UsersIsNull()
        {
            //Arrange
            int page = 2;
            int pageSize = 2;
            //Act
            var result = await _controller.GetUsersByPage(page, pageSize);
            //Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task AddUser_CorrectAndTakes5Sec()
        {
            // Arrange
            var user = new UserRequestModel { Login = "John", Password = "str", UserGroup = "User" };
            int groupId = 1;
            int stateId = 1;
            _dbMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());
            _dbMock.Setup(db => db.UserGroups).ReturnsDbSet(new List<UserGroup> { new UserGroup() { Id = groupId, Code = "User" } });
            _dbMock.Setup(db => db.UserStates).ReturnsDbSet(new List<UserState> { new UserState() { Id = stateId, Code = "Active" } });
            var start = DateTime.Now;
            // Act
            var result = await _controller.AddUser(user);

            // Assert
            var end = DateTime.Now;
            var time = end - start;
            var okResult = Assert.IsType<OkObjectResult>(result);
            var actualUser = Assert.IsType<User>(okResult.Value);
            Assert.Equal(user.Login, actualUser.Login);
            Assert.Equal(user.Password, actualUser.Password);
            Assert.Equal(user.CreatedDate, actualUser.CreatedDate);
            Assert.Equal(groupId, actualUser.UserGroupId);
            Assert.Equal(stateId, actualUser.UserStateId);
            Assert.True(time > TimeSpan.FromSeconds(5) && time < TimeSpan.FromSeconds(6));
            _dbMock.Verify(c => c.SaveChanges(), Times.Once);
        }

        [Fact]
        public async Task AddUser_UsersIsNull()
        {
            // Arrange
            var user = new UserRequestModel { Login = "John", Password = "str", UserGroup = "User" };
            // Act
            var result = await _controller.AddUser(user);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task AddUser_LoginIsBusy()
        {
            // Arrange
            var newUser = new UserRequestModel { Login = "John", Password = "str", UserGroup = "User" };
            var users = new List<User>()
            {
                new User() { Id = 1,Login = "John", Password = "str" }
            };
            _dbMock.Setup(db => db.Users).ReturnsDbSet(users);
            var message = "Уже существует пользователь с заданным логином";
            // Act
            var result = await _controller.AddUser(newUser);

            // Assert
            var badReqResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(message, badReqResult.Value);
        }

        [Fact]
        public async Task AddUser_GroupNotFound()
        {
            // Arrange
            var user = new UserRequestModel { Login = "John", Password = "str", UserGroup = "User" };
            int groupId = 1;
            int stateId = 1;
            _dbMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());
            _dbMock.Setup(db => db.UserGroups).ReturnsDbSet(new List<UserGroup>());
            _dbMock.Setup(db => db.UserStates).ReturnsDbSet(new List<UserState> { new UserState() { Id = stateId, Code = "Active" } });
            var message = "Не найдена указанная роль для пользователя";
            // Act
            var result = await _controller.AddUser(user);

            // Assert
            var badReqResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(message, badReqResult.Value);
        }

        [Fact]
        public async Task AddUser_StateNotFound()
        {
            // Arrange
            var user = new UserRequestModel { Login = "John", Password = "str", UserGroup = "User" };
            int groupId = 1;
            int stateId = 1;
            _dbMock.Setup(db => db.Users).ReturnsDbSet(new List<User>());
            _dbMock.Setup(db => db.UserGroups).ReturnsDbSet(new List<UserGroup> { new UserGroup() { Id = groupId, Code = "User" } });
            _dbMock.Setup(db => db.UserStates).ReturnsDbSet(new List<UserState>());
            var message = "Не найден активный статус в базе";
            // Act
            var result = await _controller.AddUser(user);

            // Assert
            var problem = Assert.IsType<ObjectResult>(result);
            Assert.IsType<ProblemDetails>(problem.Value);
            Assert.Equal(message, (problem.Value as ProblemDetails).Detail);
        }

        [Fact]
        public async Task AddUser_AdminIsBusy()
        {
            // Arrange
            var user = new UserRequestModel { Login = "John", Password = "str", UserGroup = "Admin" };
            int adminId = 1;
            int stateId = 1;
            var users = new List<User>()
            {
                new User() {Id = 1, Login = "1", Password = "1", UserGroupId = adminId}
            };
            _dbMock.Setup(db => db.Users).ReturnsDbSet(users);
            _dbMock.Setup(db => db.UserGroups).ReturnsDbSet(new List<UserGroup> { new UserGroup() { Id = adminId, Code = "Admin" } });
            _dbMock.Setup(db => db.UserStates).ReturnsDbSet(new List<UserState> { new UserState() { Id = stateId, Code = "Active" } });
            _dbMock.Setup(db => db.IsAdmin(It.IsAny<int>())).Returns(true);
            var message = "Админ уже существует";
            // Act
            var result = await _controller.AddUser(user);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal(message, badRequest.Value);
        }

        [Fact]
        public async Task DeleteUser_Correct()
        {
            //Arrange
            var users = new List<User>()
            {
                new User(){Id = 1, Login = "2", Password = "3"},
                new User(){Id = 2,Login = "4", Password = "5"}
            };
            int deleteId = 1;
            int blockId = 1;
            _dbMock.Setup(db => db.UserStates).ReturnsDbSet(new List<UserState> { new UserState() { Id = blockId, Code = "Blocked" } });
            var deletedUser = users.Find(n => n.Id == deleteId);
            _dbMock.Setup(db => db.Users.FindAsync(deleteId)).ReturnsAsync(deletedUser);
            //Act
            var result = await _controller.DeleteUser(deleteId);
            //Assert
            var okResult = Assert.IsType<OkResult>(result);
            _dbMock.Verify(c => c.Users.FindAsync(deleteId), Times.Once);
            Assert.Equal(deletedUser.UserStateId, blockId);
        }

        [Fact]
        public async Task DeleteUser_UsersIsNull()
        {
            //Arrange
            int deleteId = 1;
            //Act
            var result = await _controller.DeleteUser(deleteId);
            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUser_UserNotFound()
        {
            //Arrange
            var users = new List<User>()
            {
                new User(){Id = 1, Login = "2", Password = "3"},
                new User(){Id = 2,Login = "4", Password = "5"}
            };
            int deleteId = 1;
            _dbMock.Setup(n => n.Users).ReturnsDbSet(users);
            //Act
            var result = await _controller.DeleteUser(deleteId);
            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteUser_StateNotFound()
        {
            //Arrange
            var users = new List<User>()
            {
                new User(){Id = 1, Login = "2", Password = "3"},
                new User(){Id = 2,Login = "4", Password = "5"}
            };
            int deleteId = 1;
            var deletedUser = users.Find(n => n.Id == deleteId);
            _dbMock.Setup(db => db.Users.FindAsync(deleteId)).ReturnsAsync(deletedUser);
            var message = "Не найден статус заблокированного пользователя в базе";
            //Act
            var result = await _controller.DeleteUser(deleteId);
            //Assert
            var problem = Assert.IsType<ObjectResult>(result);
            Assert.IsType<ProblemDetails>(problem.Value);
            Assert.Equal(message, (problem.Value as ProblemDetails).Detail);
        }

        private void MockMethodSetup()
        {
            _dbMock.Setup(n => n.GetGroupIdByCode(It.IsAny<string>())).ReturnsAsync((string code) =>
            {
                if (_dbMock.Object.UserGroups == null)
                    return 0;
                var group = _dbMock.Object.UserGroups.SingleOrDefaultAsync(n => n.Code == code);
                if (group.Result == null)
                    return 0;
                return group.Result.Id;
            });
            _dbMock.Setup(n => n.GetStateIdByCode(It.IsAny<string>())).ReturnsAsync((string code) =>
            {
                if (_dbMock.Object.UserStates == null)
                    return 0;
                var state = _dbMock.Object.UserStates.SingleOrDefaultAsync(n => n.Code == code);
                if (state.Result == null)
                    return 0;
                return state.Result.Id;
            });
        }
    }
}
