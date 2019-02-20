using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingPlanner.Models;

namespace WeddingPlanner.Controllers
{
  public class WeddingPlannerController : Controller
  {
    private WeddingPlannerContext dbContext;
    public WeddingPlannerController(WeddingPlannerContext context)
    {
      dbContext = context;
    }


    [HttpGet("")]
    public IActionResult Index()
    {
      return View("index");
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
      return View("login");
    }

    [HttpGet("newWedding")]
    public IActionResult NewWEdding()
    {
      int? userId = HttpContext.Session.GetInt32("userId");
      ViewBag.Id = userId;
      return View("newwedding");
    }

    [HttpGet("info/{weddingId}")]
    public IActionResult WeddingInfo(int weddingId)
    {
      Wedding One = dbContext.weddings
       .Include(p => p.Guest)
       .ThenInclude(u => u.User)
       .FirstOrDefault(p => p.WeddingId == weddingId);
      ViewBag.Wedding = One;
      return View("wedinfo");
    }

    [HttpGet("dashboard")]
    public IActionResult Dashboard()
    {
      int? id = HttpContext.Session.GetInt32("userId");
      if (id is null)
      {
        ModelState.AddModelError("Email", "Please Log In");
        return View("login");
      }
      List<Wedding> AllWeddings = dbContext.weddings
        .Include(p => p.Guest)
        .ThenInclude(u => u.User)
        .ToList();
      Dashboard dash = new Dashboard
      {
          Weddings = AllWeddings,
          User = dbContext.users.FirstOrDefault(u => u.UserId == id),
          Guests = dbContext.guests.ToList()
      };
      return View("dashboard", dash);
    }

    [HttpPost("register")]
    public IActionResult SignUp(User user)
    {
      if (ModelState.IsValid)
      {
        if (dbContext.users.Any(u => u.Email == user.Email))
        {
          ModelState.AddModelError("Email", "Email already in use!");
          return View("index");
        }
        PasswordHasher<User> Hasher = new PasswordHasher<User>();
        user.Password = Hasher.HashPassword(user, user.Password);
        dbContext.Add(user);
        dbContext.SaveChanges();
        string email = user.Email;
        HttpContext.Session.SetInt32("userId", user.UserId);
        return RedirectToAction("Dashboard", "WeddingPlanner");
      }
      else
      {
        return View("index");
      }

    }

    [HttpPost("confirm")]
    public IActionResult Confirm(LoginUser userSubmission)
    {
      if (ModelState.IsValid)
      {
        var userInDb = dbContext.users.FirstOrDefault(u => u.Email == userSubmission.Email);
        if (userInDb == null)
        {
          ModelState.AddModelError("Email", "Invalid Email");
          return View("login");
        }
        PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
        PasswordVerificationResult result = hasher.VerifyHashedPassword(
            userSubmission, userInDb.Password, userSubmission.Password);
        if (result == 0)
        {
          ModelState.AddModelError("Password", "Invalid Password");
          return View("login");
        }
        HttpContext.Session.SetInt32("userId", userInDb.UserId);
        return Redirect("dashboard ");
      }
      else
      {
        return View("login");
      }
    }

    [HttpPost("createWedding")]
    public IActionResult CreateWedding(Wedding wedding)
    {
      if (ModelState.IsValid)
      {
        dbContext.Add(wedding);
        dbContext.SaveChanges();
        return RedirectToAction("Dashboard");
      }
      return View("newwedding");
    }

    [HttpGet("deletewed/{wedId}")]
    public IActionResult DeleteWedding(int wedId)
    {
      Wedding One = dbContext.weddings.FirstOrDefault(p => p.WeddingId == wedId);
      dbContext.Remove(One);
      dbContext.SaveChanges();
      return RedirectToAction("Dashboard");
    }

    [HttpPost("addguest/{userId}/{weddingId}")]
    public IActionResult AddGuest(int userId, int weddingId)
    {
      dbContext.Add(new Guest(userId, weddingId));
      dbContext.SaveChanges();
      return RedirectToAction("Dashboard");
    }

    [HttpGet("removeguest/{userId}/{weddingId}")]
    public IActionResult RemoveGuest(int userId, int weddingId)
    {
      var guest = dbContext.guests.FirstOrDefault(g => g.UserId == userId && g.WeddingID == weddingId);
      dbContext.guests.Remove(guest);
      dbContext.SaveChanges();
      return RedirectToAction("Dashboard");
    }

    [HttpGet("logout")]
    public IActionResult LogOut()
    {
      HttpContext.Session.Clear();
      return Redirect("/");
    }

  }
}
