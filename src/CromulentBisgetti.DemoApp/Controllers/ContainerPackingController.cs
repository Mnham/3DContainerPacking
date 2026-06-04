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
        public ActionResult<List<ContainerPackingResult>> Post([FromBody] ContainerPackingRequest request)
        {
            List<string> errors = ValidatePackingRequest(request);
            if (errors.Count > 0)
            {
                return BadRequest(new { errors });
            }

            return PackingService.Pack(request.Containers, request.ItemsToPack, request.AlgorithmTypeIDs);
        }

        #endregion Public Methods

        #region Private Methods

        private static List<string> ValidatePackingRequest(ContainerPackingRequest request)
        {
            List<string> errors = new List<string>();

            if (request == null)
            {
                errors.Add("Запрос на упаковку пуст.");
                return errors;
            }

            if (request.AlgorithmTypeIDs == null || request.AlgorithmTypeIDs.Count == 0)
            {
                errors.Add("Добавьте хотя бы один алгоритм упаковки.");
            }
            else
            {
                for (int i = 0; i < request.AlgorithmTypeIDs.Count; i++)
                {
                    if (request.AlgorithmTypeIDs[i] <= 0)
                    {
                        errors.Add($"Алгоритм {i + 1}: идентификатор алгоритма должен быть больше 0.");
                    }
                }
            }

            if (request.Containers == null || request.Containers.Count == 0)
            {
                errors.Add("Добавьте хотя бы один контейнер.");
            }
            else
            {
                for (int i = 0; i < request.Containers.Count; i++)
                {
                    Container container = request.Containers[i];
                    if (container == null)
                    {
                        errors.Add($"Контейнер {i + 1}: данные контейнера отсутствуют.");
                        continue;
                    }

                    if (container.Length <= 0 || container.Width <= 0 || container.Height <= 0)
                    {
                        errors.Add($"Контейнер {i + 1}: длина, ширина и высота должны быть больше 0.");
                    }
                }
            }

            if (request.ItemsToPack == null || request.ItemsToPack.Count == 0)
            {
                errors.Add("Добавьте хотя бы один предмет для упаковки.");
            }
            else
            {
                for (int i = 0; i < request.ItemsToPack.Count; i++)
                {
                    Item item = request.ItemsToPack[i];
                    if (item == null)
                    {
                        errors.Add($"Предмет {i + 1}: данные предмета отсутствуют.");
                        continue;
                    }

                    if (item.Dim1 <= 0 || item.Dim2 <= 0 || item.Dim3 <= 0 || item.Quantity <= 0)
                    {
                        errors.Add($"Предмет {i + 1}: размеры и количество должны быть больше 0.");
                    }
                }
            }

            return errors;
        }

        #endregion Private Methods
    }
}
