using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    //Include APiCOntroller so the computer knows this
    [ApiController]
    //The controller class must use ControllerBase
    public class VillaAPIController : ControllerBase
    {
        //private readonly ILogger<VillaAPIController> _logger;

        //public VillaAPIController(ILogger<VillaAPIController> logger)
        //{
        //    _logger = logger;
        //}

        public VillaAPIController()
        {

        };

        [HttpGet]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            _logger.LogInformation("Getting all villas");
            return Ok(VillaStore.villaList);
        }
        /*
            An API normally returns data
            In a form of a model
            Create a model for the data to be returned 
         */


        /*
         FirstOrDefault is a LINQ functionality to return first element of the collection or default value if requested item does not exist.

        You can give this endpoint a name so that we can use it at CreatedAtRoute method
         */
        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        /*
            [ProducesResponseType(200)]
            [ProducesResponseType(404)]
            [ProducesResponseType(400)]
         */
        public ActionResult<VillaDTO> GetVilla(int id)
        {
            if (id == 0)
            {
                _logger.LogError("Get villa error with id " + id);
                return BadRequest();
            }
            var getWithId = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if (getWithId == null)
            {
                return NotFound();
            }

            return Ok(getWithId);
        }
        /*
            Create villa via post
            In parameters you have to state the informatio is coming from body
            The reusults is of type VillaDTO
            Parameters take in the type its returning
         */
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villaDTO)
        {
            if (VillaStore.villaList.FirstOrDefault(item => item.Name.ToLower() == villaDTO.Name.ToLower()) != null)
            {
                //Create custom error
                ModelState.AddModelError("CustomeError", "Villa already exists");
                return BadRequest(ModelState);
            }
            if (villaDTO == null)
            {
                return BadRequest(villaDTO);
            }
            if (villaDTO.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            villaDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;

            //This is where you addthe new object
            VillaStore.villaList.Add(villaDTO);

            /*
             You can return a url of the object you just posted using CreatedAtRoute(Name of Route, Id of the new object as an object, Object)
             To create a object you need to use the new key word : new { id = villaDTO.id }
             */
            return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, villaDTO);
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        //When using IActionResult you do not state the return type, you return a no content
        public IActionResult DeleteVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest();
            }
            //Find villa using an ID
            var villa = VillaStore.villaList.FirstOrDefault(villa => villa.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            VillaStore.villaList.Remove(villa);
            return NoContent();

        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult UpdateVilla(int id, [FromBody] VillaDTO villaDTO)
        {
            if (villaDTO == null || id != villaDTO.Id)
            {
                return BadRequest();
            }
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            villa.Name = villaDTO.Name;
            villa.Occupancy = villaDTO.Occupancy;
            villa.Sqft = villaDTO.Sqft;
            return NoContent();
        }

        /*
         For patch to work you need to install these Nuget Packages:
         1. Microsoft.AspNetCore.JsonPatch
         2. Microsoft.AspNetCore.Mvc.NewtonsoftJson
         */
        /*These are the fields you will get for patch : 
          { 
             "op": "replace", 
             "path": "/biscuits/0/name", 
             "value": "Chocolate Digestive" 
          }

        op - the name of the opperation
        path - the key name of what you want to replace e.g /name
        value - the value you want to replace it with
        */
        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if (villa == null)
            {
                return NotFound();
            }
            //Apply patch to villa, and save any errors to ModelState
            patchDTO.ApplyTo(villa, ModelState);
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            return NoContent();
        }
    }
}
