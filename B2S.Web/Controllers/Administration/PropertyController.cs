using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using B2S.Core.Entities;
using B2S.Infrastructure.Data;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Http;
using System.IO;
using B2S.Web.Extensions;
using B2S.Core.Common;
using Microsoft.Extensions.Options;
using B2S.Core.Utilities;

namespace B2S.Web.Controllers
{
    public class PropertyController : Controller
    {
        private readonly AppDbContext _context;
        private readonly PaginationOptions _paginationOptions;

        public PropertyController(AppDbContext context, IOptions<PaginationOptions> paginationOptions)
        {
            _context = context;
            _paginationOptions = paginationOptions.Value;
        }

        // GET: Property
        public async Task<IActionResult> Index(string search, int? page)
        {           
            ViewData["CurrentSearch"] = search;
            var properties = _context.Property.Include(c => c.Vendor).Select(c => c);

            if (!string.IsNullOrEmpty(search))
            {
                properties = properties.Where(p => p.PropertyName.ToUpper().Contains(search.ToUpper()));
            }

            int pageSize = _paginationOptions.PageSize;

            return View(await PaginatedList<Property>.CreateAsync(properties, page ?? 1, pageSize));
        }

        // GET: Property/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @property = await _context.Property
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.PropertyId == id);
            if (@property == null)
            {
                return NotFound();
            }

            return View(@property);
        }

        #region Step 1: Property Setup

        // GET: Property/Create
        public async Task<IActionResult> Create(string vendorId, string id)
        {
            var check = await _context.Property.FindAsync(id);
            var selected = await _context.Vendor.FindAsync(vendorId);
            ViewData["IsPopup"] = false;
            if (selected != null)
            {
                ViewData["IsPopup"] = true;
            }
            if (check == null)
            {
                ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorName", vendorId);
                Property property = new Property
                {
                    VendorId = vendorId,
                };
                return View(property);
            }
            else
            {
                ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorName", check.VendorId);
                return View(check);

            }           
        }

        // POST: Property/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PropertyId,PropertyName,PropertyType,VendorId,Address,City,Province,Postcode,Country,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] Property @property)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrEmpty(property.PropertyId))
                {
                    property.CreatedAt = DateTime.UtcNow;
                    property.CreatedBy = User.Identity.GetUserId();
                    _context.Add(property);
                    await _context.SaveChangesAsync();
                    return View(property);
                }
                property.ModifiedAt = DateTime.UtcNow;
                property.ModifiedBy = User.Identity.GetUserId();
                _context.Update(@property);
                await _context.SaveChangesAsync();
                return RedirectToAction("Building", new { propertyId = property.PropertyId });
            }
            ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorName", @property.VendorId);
            return View(@property);
        }

        public async Task<IActionResult> CreateRoomType(string id, string propertyId)
        {
            var roomType = await _context.RoomType.FindAsync(id);
            if (roomType != null)
            {
                return View(roomType);
            }

            RoomType newRoomType = new RoomType
            {
                PropertyId = propertyId
            };
            return View(newRoomType);
        }

        public async Task<IActionResult> CreateDocument(string id, string propertyId)
        {
            var document = await _context.PropertyDocument.FindAsync(id);
            if (document != null)
            {               
                return View(document);
            }

            PropertyDocument newDocument = new PropertyDocument
            {
                PropertyId = propertyId
            };
            return View(newDocument);
        }

        public async Task<IActionResult> GetViewDocument(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.PropertyDocument.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var documentName = document.DocumentName;
            if (!string.IsNullOrEmpty(document.FileName))
            {
                int extensionIndex = document.FileName.LastIndexOf(".");
                var extenstion = document.FileName.Substring(extensionIndex);
                documentName = string.Format("{0}{1}", document.DocumentName, extenstion);

            }

            Response.Headers.Add("Content-Disposition", "inline;filename=" + documentName);

            return File(document.FileBody, document.FileType);
        }

        public async Task<IActionResult> DownLoadDocument(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var document = await _context.PropertyDocument.FindAsync(id);
            if (document == null)
            {
                return NotFound();
            }

            var documentName = document.DocumentName;
            if (!string.IsNullOrEmpty(document.FileName))
            {
                int extensionIndex = document.FileName.LastIndexOf(".");
                var extenstion = document.FileName.Substring(extensionIndex);
                documentName = string.Format("{0}{1}", document.DocumentName, extenstion);
            }

            return File(document.FileBody, document.FileType, documentName);
        }

        #endregion

        #region Step 2 : Building Setup

        public async Task<IActionResult> Building(string propertyId)
        {
            ViewData["PropertyId"] = propertyId;
            var buildings = _context.Building.Where(b => b.PropertyId == propertyId);
            return View(await buildings.ToListAsync());
        }

        public async Task<IActionResult> AddBuilding(string id, string propertyId)
        {
            var building = await _context.Building.FindAsync(id);
            if (building != null)
            {
                return View(building);
            }

            Building newBuilding = new Building
            {
                PropertyId = propertyId
            };

            return View(newBuilding);
        }

        [HttpPost]
        public async Task<IActionResult> AddBuilding(Building building)
        {
            if (string.IsNullOrEmpty(building.BuildingId))
            {
                building.CreatedAt = DateTime.UtcNow;
                building.CreatedBy = User.Identity.GetUserId();
                _context.Add(building);

                var formFiles = Request.Form.Files;
                foreach (var formFile in formFiles)
                {
                    if (!formFile.IsImage())
                    {
                        return Json(new { success = false, message = "Invalid image." });
                    }

                    byte[] newImageFile;
                    using (var stream = new MemoryStream())
                    {
                        await formFile.CopyToAsync(stream);
                        newImageFile = stream.ToArray();
                        BuildingImage buildingImage = new BuildingImage
                        {
                            BuildingId = building.BuildingId,
                            ImageFile = newImageFile,
                            IsDefault = false,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = User.Identity.GetUserId()
                        };
                        _context.Add(buildingImage);
                    }
                }
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Add success." });
            }
            else
            {
                building.ModifiedAt = DateTime.UtcNow;
                building.ModifiedBy = User.Identity.GetUserId();
                _context.Update(building);
                //images have been created when trigger upload in addbuiling.cshtml

                //var formFiles = Request.Form.Files;
                //foreach (var formFile in formFiles)
                //{
                //    if (!formFile.IsImage())
                //    {
                //        return Json(new { success = false, message = "Invalid image." });
                //    }

                //    byte[] newImageFile;
                //    using (var stream = new MemoryStream())
                //    {
                //        await formFile.CopyToAsync(stream);
                //        newImageFile = stream.ToArray();
                //        BuildingImage buildingImage = new BuildingImage
                //        {
                //            BuildingId = building.BuildingId,
                //            ImageFile = newImageFile,
                //            IsDefault = false,
                //            CreatedAt = DateTime.UtcNow,
                //            CreatedBy = User.Identity.GetUserId()
                //        };
                //        _context.Add(buildingImage);
                //    }
                //}
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Update success." });
            }
          
        }

        [HttpDelete]
        public async Task<IActionResult> DelBuilding([FromRoute] string id)
        {
            var building = await _context.Building.FindAsync(id);
            if (building == null)
            {
                return Json(new { success = false, message = "Building not found." });
            }
            _context.Remove(building);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Delete success." });
        }

        #endregion

        #region Step 3 : Room/Bed Setup

        public async Task<IActionResult> Room (string propertyId)
        {
            ViewData["PropertyId"] = propertyId;
            var buildings = _context.Building.OrderBy(b => b.Name).Where(r => r.PropertyId == propertyId);
            return View(await buildings.ToListAsync());
        }

        public async Task<IActionResult> AddRoom(string id, string buildingId)
        {
            var room = await _context.Room.FindAsync(id);
            if (room != null)
            {
                ViewData["RoomTypeId"] = new SelectList(_context.RoomType.Where(r => r.PropertyId == room.PropertyId), "RoomTypeId", "RoomTypeName", room.RoomTypeId);
                return View(room);
            }

            var building = await _context.Building.FindAsync(buildingId);          

            Room newRoom = new Room
            {
                BuildingId = buildingId,
                PropertyId = building?.PropertyId
            };
            ViewData["RoomTypeId"] = new SelectList(_context.RoomType.Where(r => r.PropertyId == building.PropertyId), "RoomTypeId", "RoomTypeName");
            return View(newRoom);
        }

        [HttpPost]
        public async Task<IActionResult> AddRoom(Room room)
        {
            if (string.IsNullOrEmpty(room.RoomId))
            {
                room.CreatedAt = DateTime.UtcNow;
                room.CreatedBy = User.Identity.GetUserId();
                _context.Add(room);                             
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Add success." });
            }
            else
            {
                room.ModifiedAt = DateTime.UtcNow;
                room.ModifiedBy = User.Identity.GetUserId();
                _context.Update(room);               
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Update success." });
            }

        }

        [HttpDelete]
        public async Task<IActionResult> DelRoom([FromRoute] string id)
        {
            var room = await _context.Room.FindAsync(id);
            if (room == null)
            {
                return Json(new { success = false, message = "Room not found." });
            }
            _context.Remove(room);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Delete success." });
        }

        public async Task<IActionResult> ManageBed(string roomId, string id)
        {
            var bed = await _context.Bed.Include(b => b.Room.RoomType.Property).Include(b => b.Room.Building).FirstOrDefaultAsync(b => b.BedId == id);
            if (bed != null)
            {                
                return View(bed);
            }

            var room = await _context.Room.Include(r => r.Building).Include(r => r.RoomType.Property).FirstOrDefaultAsync(m => m.RoomId == roomId);            
            Bed newBed = new Bed
            {
                RoomId = roomId,
                Room = room
            };
            return View(newBed);
        }


        #endregion

        // GET: Property/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @property = await _context.Property.FindAsync(id);
            if (@property == null)
            {
                return NotFound();
            }
            ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorName", @property.VendorId);
            return View(@property);
        }

        // POST: Property/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("PropertyId,PropertyName,PropertyType,VendorId,Address,City,Province,Postcode,Country,CreatedAt,CreatedBy,ModifiedAt,ModifiedBy,DeletedAt,DeletedBy,IsDeleted")] Property @property)
        {
            if (id != @property.PropertyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(@property);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropertyExists(@property.PropertyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["VendorId"] = new SelectList(_context.Vendor, "VendorId", "VendorName", @property.VendorId);
            return View(@property);
        }

        // GET: Property/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @property = await _context.Property
                .Include(p => p.Vendor)
                .FirstOrDefaultAsync(m => m.PropertyId == id);
            if (@property == null)
            {
                return NotFound();
            }

            return View(@property);
        }

        // POST: Property/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var @property = await _context.Property.FindAsync(id);
            _context.Property.Remove(@property);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PropertyExists(string id)
        {
            return _context.Property.Any(e => e.PropertyId == id);
        }
    }
}
