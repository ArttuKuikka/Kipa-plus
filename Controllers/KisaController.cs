﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Kipa_plus.Data;
using Kipa_plus.Models;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel;
using Kipa_plus.Models.DynamicAuth;

namespace Kipa_plus.Controllers
{
    [Route("[controller]")]
    [Static]
    [Authorize]
    public class KisaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IRoleAccessStore _roleAccessStore;
        private readonly DynamicAuthorizationOptions _authorizationOptions;

        public KisaController(ApplicationDbContext context, IRoleAccessStore roleAccessStore, DynamicAuthorizationOptions authorizationOptions)
        {
            _context = context;
            _roleAccessStore = roleAccessStore;
            _authorizationOptions = authorizationOptions;
        }
        [HttpGet("{kisaId:int}/LiittymisId")]
        [DisplayName("Näytä liittymisID")]
        public async Task<IActionResult> LiittymisId(int kisaId)
        {
            if (kisaId == null || _context.Kisa == null)
            {
                return NotFound();
            }

            var kisa = await _context.Kisa.FindAsync(kisaId);

            if(kisa.LiittymisId == null)
            {
                kisa.LiittymisId = Guid.NewGuid().ToString();
                _context.Update(kisa);
                _context.SaveChanges();
            }
            (string, int) returnitem = (kisa.LiittymisId, kisaId);


            return View("LiittymisId",returnitem);
        }

        [HttpGet("{kisaId:int}/LiittymisIdUudelleenluonti")]
        [DisplayName("Uudelleenluo liittymisID")]
        public async Task<IActionResult> LiittymisIdUudelleenluonti(int kisaId)
        {
            if (kisaId == null || _context.Kisa == null)
            {
                return NotFound();
            }

            var kisa = await _context.Kisa.FindAsync(kisaId);

            kisa.LiittymisId = Guid.NewGuid().ToString();
            _context.Update(kisa);
            _context.SaveChanges();


            return Redirect($"/Kisa/{kisaId}/LiittymisId");
        }

        [HttpGet("{kisaId:int}/Lataukset")]
        [DisplayName("Latausvaihtoedot")]
        public async Task<IActionResult> Lataukset(int kisaId)
        {
            if (kisaId == null || _context.Kisa == null)
            {
                return NotFound();
            }


            return View(kisaId);
        }

        [DisplayName("Luo Rasti")]
        [HttpGet("{kisaId:int}/LuoRasti")]
        // GET: Rasti/Luo
        public IActionResult LuoRasti(int kisaId)
        {

            ViewBag.Kisat = _context.Kisa.ToList();

            return View(new Rasti() { KisaId = kisaId });
        }

        // POST: Rasti/Luo
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("LuoRasti")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LuoRasti([Bind("Id,SarjaId,KisaId,Nimi,OhjeId")] Rasti rasti)
        {
            ViewBag.Sarjat = _context.Sarja.ToList();
            ViewBag.Kisat = _context.Kisa.ToList();
            if (ModelState.IsValid)
            {
                if (_context.Rasti.Where(x => x.Nimi == rasti.Nimi).Where(x => x.KisaId == rasti.KisaId).Any())
                {
                    ViewBag.Error = "Rasti tällä nimellä on jo olemassa";
                    return View(rasti);
                }

                _context.Add(rasti);
                await _context.SaveChangesAsync();
                return Redirect("/Kisa/" + rasti.KisaId + "/Rastit");
            }
            return View(rasti);
        }


        // GET: Rasti/Delete/5
        [HttpGet("{kisaId:int}/PoistaRasti")]
        [DisplayName("Poista rasti")]
        public async Task<IActionResult> PoistaRasti(int? RastiId)
        {
            if (RastiId == null || _context.Rasti == null)
            {
                return NotFound();
            }

            var rasti = await _context.Rasti
                .FirstOrDefaultAsync(m => m.Id == RastiId);
            if (rasti == null)
            {
                return NotFound();
            }

            return View(rasti);
        }

        // POST: Rasti/Delete/5
        [HttpPost("{kisaId:int}/PoistaRasti"), ActionName("PoistaRasti")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RastiDeleteConfirmed(Rasti viewModel)
        {
            if (_context.Rasti == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Rasti'  is null.");
            }
            var rasti = await _context.Rasti.FindAsync(viewModel.Id);

            if (rasti != null)
            {
                _context.Rasti.Remove(rasti);

                var KisaId = rasti.KisaId;

                await _context.SaveChangesAsync();
                return Redirect("/Kisa/" + KisaId + "/Rastit");
            }

            return BadRequest();
        }

        private bool RastiExists(int? id)
        {
            return (_context.Rasti?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        // GET: Kisa
        [HttpGet("{kisaId:int}/")]
        [DisplayName("Etusivu")]
        public async Task<IActionResult> Index(int kisaId)
        {
            if(kisaId == 0)
            {
                return Redirect("/");
            }
            Kisa? kisa; 
            if(User.Identity.Name == _authorizationOptions.DefaultAdminUser)
            {
                kisa = await _context.Kisa
                .FirstOrDefaultAsync(m => m.Id == kisaId);
            }
            else
            {
                kisa = await _context.Kisa
                .FirstOrDefaultAsync(m => m.Id == kisaId);

                var roles = await (
               from usr in _context.Users
               join userRole in _context.UserRoles on usr.Id equals userRole.UserId
               join role in _context.Roles on userRole.RoleId equals role.Id
               where usr.UserName == User.Identity.Name
               select role.Id.ToString()
           ).ToArrayAsync();

                var rastit = await _roleAccessStore.HasAccessToRastiIdsAsync(roles);

                kisa.OikeusRasteihin = rastit;

            }
            return View(kisa);
        }

        

        // GET: Kisa/Luo
        [HttpGet("Luo")]
        public IActionResult Luo()
        {
            return View();
        }

        // POST: Kisa/Luo
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("Luo")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Luo([Bind("Id,Nimi")] Kisa kisa)
        {
           
            if (ModelState.IsValid)
            {
                _context.Add(kisa);
                await _context.SaveChangesAsync();
                return Redirect("/");
            }
            return View(kisa);
        }

        // GET: Kisa/Edit/5
        [HttpGet("{kisaId:int}/Edit")]
        [DisplayName("Muokkaa")]
        public async Task<IActionResult> Edit(int kisaId)
        {
            if (kisaId == null || _context.Kisa == null)
            {
                return NotFound();
            }

            var kisa = await _context.Kisa.FindAsync(kisaId);
            if (kisa == null)
            {
                return NotFound();
            }
            return View(kisa);
        }

        // POST: Kisa/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("{kisaId:int}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nimi")] Kisa kisa)
        {
            if (id != kisa.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kisa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KisaExists(kisa.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect("/Kisa/" + kisa.Id.ToString());
            }
            return View(kisa);
        }

        // GET: Kisa/Delete/5
        [HttpGet("{kisaId:int}/Delete")]
        [DisplayName("Poista")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Kisa == null)
            {
                return NotFound();
            }

            var kisa = await _context.Kisa
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kisa == null)
            {
                return NotFound();
            }

            return View(kisa);
        }

        // POST: Kisa/Delete/5
        [HttpPost("{kisaId:int}/Delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Kisa == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Kisa'  is null.");
            }
            var kisa = await _context.Kisa.FindAsync(id);
            if (kisa != null)
            {
                _context.Kisa.Remove(kisa);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KisaExists(int? id)
        {
          return (_context.Kisa?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet("{kisaId:int}/Sarjat")]
        [DisplayName("Listaa sarjat")]
        public async Task<IActionResult> Sarjat(int kisaId)
        {
            if (kisaId == 0 || _context.Sarja == null)
            {
                return NotFound();
            }

            var sarjat = _context.Sarja
                .Where(m => m.KisaId == kisaId);
            if (sarjat == null)
            {
                return NotFound();
            }
            ViewBag.KisaId = kisaId;
            return View(sarjat);
        }

        [HttpGet("{kisaId:int}/Vartiot")]
        [DisplayName("Listaa vartiot")]
        public async Task<IActionResult> Vartiot(int kisaId)
        {
            if (kisaId == 0 || _context.Vartio == null)
            {
                return NotFound();
            }

            var vartiot = _context.Vartio
                .Where(m => m.KisaId == kisaId);
            if (vartiot == null)
            {
                return NotFound();
            }
            ViewBag.KisaId = kisaId;
            ViewBag.Sarjat = _context.Sarja.Where(x=> x.KisaId == kisaId).ToList();
            return View(vartiot);
        }

        [HttpGet("{kisaId:int}/Rastit")]
        [DisplayName("Listaa rastit")]
        public async Task<IActionResult> Rastit(int kisaId)
        {
            if (kisaId == 0 || _context.Rasti == null)
            {
                return NotFound();
            }

            var roles = await (
                from usr in _context.Users
                join userRole in _context.UserRoles on usr.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where usr.UserName == User.Identity.Name
                select role.Id.ToString()
            ).ToArrayAsync();

            var rastitjoihinoikeudet = await _roleAccessStore.HasAccessToRastiIdsAsync(roles);

            IQueryable rastit;

            if(User.Identity.Name == _authorizationOptions.DefaultAdminUser)
            {
                rastit = _context.Rasti
                .Where(m => m.KisaId == kisaId);
            }
            else
            {
                rastit = _context.Rasti
                .Where(m => m.KisaId == kisaId).Where(x => rastitjoihinoikeudet.Contains((int)x.Id));
            }


            if (rastit == null)
            {
                return NotFound();
            }
            ViewData["Sarjat"] = _context.Sarja.ToList();

            ViewBag.KisaId = kisaId;
            return View(rastit);
        }
    }
}
