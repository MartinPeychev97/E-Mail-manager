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
        private static readonly log4net.ILog log =
        log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IUserServices _userService;

        public UserController(IUserServices userService)
        {
            this._userService = userService;
        }

        //For displaying all users (with pagination of 10 per page)
        public async Task<IActionResult> Index(int? currentPage, string search = null)
        {
            var currPage = currentPage ?? 1;

            int totalPages = await _userService.GetPageCount(10);

            IEnumerable<User> usersAllResults = null;

            if (!string.IsNullOrEmpty(search))
            {
                //For user search
                usersAllResults = await _userService.SearchUsers(search, currPage);
                log.Info($"User searched for {search} user.");
            }
            else
            {
                usersAllResults = _userService.GetAll(currPage);
                log.Info("Displayed all user list.");
            }

            var userListing = usersAllResults
                .Select(m => UserMapper.MapFromUser(m, _userService));
            var userModel = UserMapper.MapFromUserIndex(userListing, currPage, totalPages);

            //For pagination buttons and distribution
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
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = _userService.GetUserById(userId);
            var userModel = UserMapper.MapFromUser(user, _userService);

            log.Info($"User with id: {currentUserId}, opened user detail page. User Id: {userId}");

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