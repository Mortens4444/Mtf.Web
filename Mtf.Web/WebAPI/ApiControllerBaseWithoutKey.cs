using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mtf.Database.Interfaces;
using Mtf.Web.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Mtf.Web.WebAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBaseWithoutKey<TDto, TModel, TRepository, TConverter> : ControllerBase
        where TDto : class
        where TModel : class
        where TRepository : IRepository<TModel>
        where TConverter : IConverter<TModel, TDto>
    {
        protected ILogger Logger { get; }
        protected TRepository Repository { get; }
        protected TConverter Converter { get; }

        protected ApiControllerBaseWithoutKey(
            ILogger logger,
            TRepository repository,
            TConverter converter)
        {
            Logger = logger;
            Repository = repository;
            Converter = converter;
        }

        [HttpGet]
        public virtual ActionResult<IEnumerable<TDto>> GetAll()
        {
            Logger.LogInformation("Getting all {Entity}", typeof(TModel).Name);
            var models = Repository.SelectAll();
            var dtos = models?.Select(Converter.ToDto);
            return Ok(dtos);
        }

        [HttpPost]
        public virtual ActionResult<TDto> Create([FromBody] TDto dto)
        {
            Logger.LogInformation("Creating a new {Entity}", typeof(TModel).Name);
            var model = Converter.ToModel(dto);
            if (model == null)
            {
                return BadRequest("Invalid data");
            }

            Repository.Insert(model);
            return Ok(dto);
        }

        [HttpPut]
        public virtual IActionResult Update([FromBody] TDto dto)
        {
            Logger.LogInformation("Updating {Entity}", typeof(TModel).Name);
            var model = Converter.ToModel(dto);
            if (model == null)
            {
                return BadRequest("Invalid data");
            }

            Repository.Update(model);
            return NoContent();
        }

        //[HttpDelete]
        //public virtual IActionResult Delete([FromBody] TDto dto)
        //{
        //    Logger.LogInformation("Deleting {Entity}", typeof(TModel).Name);
        //    var model = Converter.ToModel(dto);
        //    if (model == null)
        //    {
        //        return BadRequest("Invalid data");
        //    }

        //    Repository.Delete(model);
        //    return NoContent();
        //}
    }
}
