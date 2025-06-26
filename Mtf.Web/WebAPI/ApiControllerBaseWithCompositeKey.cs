using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Mtf.Web.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Mtf.Web.WebAPI
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBaseWithCompositeKey<TDto, TModel, TKey, TRepository, TConverter> : ControllerBase
        where TDto : class
        where TModel : class
        where TKey : class
        where TRepository : IRepositoryWithKey<TModel, TKey>
        where TConverter : IConverter<TModel, TDto>
    {
        protected ILogger Logger { get; }
        protected TRepository Repository { get; }
        protected TConverter Converter { get; }

        protected ApiControllerBaseWithCompositeKey(ILogger logger, TRepository repository, TConverter converter)
        {
            Logger = logger;
            Repository = repository;
            Converter = converter;
        }

        [HttpGet]
        public virtual ActionResult<IEnumerable<TDto>> GetAll()
        {
            Logger.LogInformation($"Getting all {typeof(TModel).Name}s");
            var models = Repository.SelectAll();
            var result = models?.Select(Converter.ToDto);
            return Ok(result);
        }

        [HttpPost("get-by-key")]
        public virtual ActionResult<TDto> GetByKey([FromBody] TKey key)
        {
            Logger.LogInformation("Getting {Entity} by composite key", typeof(TModel).Name);
            var model = Repository.SelectByKey(key);
            if (model == null)
            {
                return NotFound();
            }

            var dto = Converter.ToDto(model);
            return Ok(dto);
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

        [HttpDelete]
        public virtual IActionResult Delete([FromBody] TKey key)
        {
            Logger.LogInformation("Deleting {Entity} with composite key", typeof(TModel).Name);

            var existing = Repository.SelectByKey(key);
            if (existing == null)
            {
                return NotFound();
            }

            Repository.DeleteByKey(key);
            return NoContent();
        }
    }
}
