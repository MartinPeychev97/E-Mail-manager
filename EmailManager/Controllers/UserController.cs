using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using EmailManager.Data.Implementation;
using EmailManager.Mappers;
using EmailManager.Services.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmailManager.Controllers
{
    public class UserController : Controller
    {

        private readonly IUserServices _userService;
        private static readonly log4net.ILog log =
      log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public UserController(IUserServices userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index(int? currentPage, string search = null)
        {
            var currPage = currentPage ?? 1;

            int totalPages = await _userService.GetPageCount(10);

            IEnumerable<User> emailAllResults = null;

            if (!string.IsNullOrEmpty(search))
            {
                emailAllResults = await _userService.SearchUsers(search, currPage);
                log.Info($"User searched for {search} user.");
            }
            else
            {
                emailAllResults = _userService.GetAll(currPage);
                log.Info("Displayed all user list.");
            }

            var userListing = emailAllResults
                .Select(m => UserMapper.MapFromUser(m, _userService));
            var userModel = UserMapper.MapFromUserIndex(userListing, currPage, totalPages);

            userModel.CurrentPage = currPage;
            userModel.TotalPages = totalPages;

            if (totalPages > currPage)
            {
                userModel.NextPage = currPage + 1;
            }

            if (currPage > 1)
            {
                userModel.PreviousPage = currPage - 1;
            }

            return View(userModel);
        }

        public IActionResult Detail(string userId)
        {
           // var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userService.GetUserById(userId);
            var userModel = UserMapper.MapFromUser(user, _userService);


            return View("Detail", userModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public IActionResult BanUser(string userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userService.GetUserById(userId);
            _userService.BanUser(userId);

            var userModel = UserMapper.MapFromUser(user, _userService);

            log.Info($"User with id: {currentUserId}, banned user with Id: {userId}");

            return View("Detail", userModel);
        }
    }
}