using CromulentBisgetti.ContainerPacking;
using CromulentBisgetti.ContainerPacking.Entities;
using CromulentBisgetti.DemoApp.Models;

using Microsoft.AspNetCore.Mvc;

using System.Collections.Generic;

namespace CromulentBisgetti.DemoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContainerPackingController : ControllerBase
    {
        #region Public Methods

        // POST api/values
        [HttpPost]
        public ActionResult<List<ContainerPackingResult>> Post([FromBody] ContainerPackingRequest request) =>
            PackingService.Pack(request.Containers, request.ItemsToPack, request.AlgorithmTypeIDs);

        #endregion Public Methods
    }
}