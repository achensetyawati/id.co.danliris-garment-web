﻿using Barebone.Controllers;
using Manufactures.Domain.GarmentCuttingOuts.Commands;
using Manufactures.Domain.GarmentCuttingOuts.Repositories;
using Manufactures.Domain.GarmentCuttingIns.Repositories;
using Manufactures.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Manufactures.Domain.GarmentSewingDOs;
using Manufactures.Domain.GarmentSewingDOs.Repositories;
using Newtonsoft.Json;
using Infrastructure.Data.EntityFrameworkCore.Utilities;
using Manufactures.Application.GarmentCuttingOuts.Queries;

namespace Manufactures.Controllers.Api
{
    [ApiController]
    [Authorize]
    [Route("cutting-outs")]
    public class GarmentCuttingOutController : ControllerApiBase
    {
        private readonly IGarmentCuttingOutRepository _garmentCuttingOutRepository;
        private readonly IGarmentCuttingOutItemRepository _garmentCuttingOutItemRepository;
        private readonly IGarmentCuttingOutDetailRepository _garmentCuttingOutDetailRepository;
        private readonly IGarmentCuttingInRepository _garmentCuttingInRepository;
        private readonly IGarmentCuttingInItemRepository _garmentCuttingInItemRepository;
        private readonly IGarmentCuttingInDetailRepository _garmentCuttingInDetailRepository;
        private readonly IGarmentSewingDORepository _garmentSewingDORepository;
        private readonly IGarmentSewingDOItemRepository _garmentSewingDOItemRepository;

        public GarmentCuttingOutController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _garmentCuttingOutRepository = Storage.GetRepository<IGarmentCuttingOutRepository>();
            _garmentCuttingOutItemRepository = Storage.GetRepository<IGarmentCuttingOutItemRepository>();
            _garmentCuttingOutDetailRepository = Storage.GetRepository<IGarmentCuttingOutDetailRepository>();
            _garmentCuttingInRepository = Storage.GetRepository<IGarmentCuttingInRepository>();
            _garmentCuttingInItemRepository = Storage.GetRepository<IGarmentCuttingInItemRepository>();
            _garmentCuttingInDetailRepository = Storage.GetRepository<IGarmentCuttingInDetailRepository>();
            _garmentSewingDORepository = Storage.GetRepository<IGarmentSewingDORepository>();
            _garmentSewingDOItemRepository = Storage.GetRepository<IGarmentSewingDOItemRepository>();
        }

        [HttpGet]
        public async Task<IActionResult> Get(int page = 1, int size = 25, string order = "{}", [Bind(Prefix = "Select[]")]List<string> select = null, string keyword = null, string filter = "{}")
        {
            VerifyUser();

            var query = _garmentCuttingOutRepository.Read(page, size, order, "", filter);
            var total = query.Count();
            query = query.Skip((page - 1) * size).Take(size);

            var garmentCuttingOutDto = _garmentCuttingOutRepository.Find(query).Select(o => new GarmentCuttingOutListDto(o)).ToArray();
            var garmentCuttingOutItemDto = _garmentCuttingOutItemRepository.Find(_garmentCuttingOutItemRepository.Query).Select(o => new GarmentCuttingOutItemDto(o)).ToList();
            var garmentCuttingOutItemDtoArray = _garmentCuttingOutItemRepository.Find(_garmentCuttingOutItemRepository.Query).Select(o => new GarmentCuttingOutItemDto(o)).ToArray();
            var garmentCuttingOutDetailDto = _garmentCuttingOutDetailRepository.Find(_garmentCuttingOutDetailRepository.Query).Select(o => new GarmentCuttingOutDetailDto(o)).ToList();
            var garmentCuttingOutDetailDtoArray = _garmentCuttingOutDetailRepository.Find(_garmentCuttingOutDetailRepository.Query).Select(o => new GarmentCuttingOutDetailDto(o)).ToArray();

            Parallel.ForEach(garmentCuttingOutDto, itemDto =>
            {
                var garmentCuttingOutItems = garmentCuttingOutItemDto.Where(x => x.CutOutId == itemDto.Id).ToList();


                itemDto.Items = garmentCuttingOutItems;

                Parallel.ForEach(itemDto.Items, detailDto =>
                {
                    var garmentCuttingOutDetails = garmentCuttingOutDetailDto.Where(x => x.CutOutItemId == detailDto.Id).ToList();
                    detailDto.Details = garmentCuttingOutDetails;

                    detailDto.Details = detailDto.Details.OrderBy(x => x.Id).ToList();
                });

                itemDto.Items = itemDto.Items.OrderBy(x => x.Id).ToList();

                itemDto.Products = itemDto.Items.Select(i => i.Product.Code).ToList();
                itemDto.TotalCuttingOutQuantity = itemDto.Items.Sum(i => i.Details.Sum(d => d.CuttingOutQuantity));
                itemDto.TotalRemainingQuantity = itemDto.Items.Sum(i => i.Details.Sum(d => d.RemainingQuantity));
            });

            if (!string.IsNullOrEmpty(keyword))
            {
                garmentCuttingOutItemDtoArray = garmentCuttingOutItemDto.Where(x => x.Product.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)).ToArray();
                List<GarmentCuttingOutListDto> ListTemp = new List<GarmentCuttingOutListDto>();
                foreach (var a in garmentCuttingOutItemDtoArray)
                {
                    var temp = garmentCuttingOutDto.Where(x => x.Id.Equals(a.CutOutId)).ToArray();
                    foreach (var b in temp)
                    {
                        ListTemp.Add(b);
                    }
                }

                var garmentCuttingOutDtoList = garmentCuttingOutDto.Where(x => x.CutOutNo.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                    || x.Unit.Code.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                    || x.RONo.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                    || x.Article.Contains(keyword, StringComparison.OrdinalIgnoreCase)
                                    ).ToList();

                var i = 0;
                foreach (var data in ListTemp)
                {
                    i = 0;
                    foreach (var item in garmentCuttingOutDtoList)
                    {
                        if (data.Id == item.Id)
                        {
                            i++;
                        }
                    }
                    if (i == 0)
                    {
                        garmentCuttingOutDtoList.Add(data);
                    }
                }
                var garmentCuttingOutDtoListArray = garmentCuttingOutDtoList.ToArray();
                if (order != "{}")
                {
                    Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
                    garmentCuttingOutDtoListArray = QueryHelper<GarmentCuttingOutListDto>.Order(garmentCuttingOutDtoList.AsQueryable(), OrderDictionary).ToArray();
                }
                else
                {
                    garmentCuttingOutDtoListArray = garmentCuttingOutDtoList.OrderByDescending(x => x.LastModifiedDate).ToArray();
                }

                //garmentCuttingOutDtoListArray = garmentCuttingOutDtoListArray.Take(size).Skip((page - 1) * size).ToArray();

                await Task.Yield();
                return Ok(garmentCuttingOutDtoListArray, info: new
                {
                    page,
                    size,
                    total
                });
            }
            else
            {
                if (order != "{}")
                {
                    Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(order);
                    garmentCuttingOutDto = QueryHelper<GarmentCuttingOutListDto>.Order(garmentCuttingOutDto.AsQueryable(), OrderDictionary).ToArray();
                }
                else
                {
                    garmentCuttingOutDto = garmentCuttingOutDto.OrderByDescending(x => x.LastModifiedDate).ToArray();
                }

                //garmentCuttingOutDto = garmentCuttingOutDto.Take(size).Skip((page - 1) * size).ToArray();

                await Task.Yield();
                return Ok(garmentCuttingOutDto, info: new
                {
                    page,
                    size,
                    total
                });
            }

            //List<GarmentCuttingOutListDto> garmentCuttingOutListDtos = _garmentCuttingOutRepository.Find(query).Select(cutOut =>
            //{
            //    var items = _garmentCuttingOutItemRepository.Query.Where(o => o.CutOutId == cutOut.Identity).Select(cutOutItem => new
            //    {
            //        cutOutItem.ProductCode,
            //        details = _garmentCuttingOutDetailRepository.Query.Where(o => o.CutOutItemId == cutOutItem.Identity).Select(cutOutDetail => new
            //        {
            //            cutOutDetail.CuttingOutQuantity,
            //            cutOutDetail.RemainingQuantity,
            //        })
            //    }).ToList();

            //    return new GarmentCuttingOutListDto(cutOut)
            //    {
            //        Products = items.Select(i => i.ProductCode).ToList(),
            //        TotalCuttingOutQuantity = items.Sum(i => i.details.Sum(d => d.CuttingOutQuantity)),
            //        TotalRemainingQuantity = items.Sum(i => i.details.Sum(d => d.RemainingQuantity))
            //    };
            //}).ToList();

            //await Task.Yield();
            //return Ok(garmentCuttingOutListDtos, info: new
            //{
            //    page,
            //    size,
            //    count
            //});
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            Guid guid = Guid.Parse(id);

            VerifyUser();

            GarmentCuttingOutDto garmentCuttingOutDto = _garmentCuttingOutRepository.Find(o => o.Identity == guid).Select(cutOut => new GarmentCuttingOutDto(cutOut)
            {
                Items = _garmentCuttingOutItemRepository.Find(o => o.CutOutId == cutOut.Identity).Select(cutOutItem => new GarmentCuttingOutItemDto(cutOutItem)
                {
                    Details = _garmentCuttingOutDetailRepository.Find(o => o.CutOutItemId == cutOutItem.Identity).Select(cutOutDetail => new GarmentCuttingOutDetailDto(cutOutDetail)
                    {
                        //PreparingRemainingQuantity = _garmentPreparingItemRepository.Query.Where(o => o.Identity == cutInDetail.PreparingItemId).Select(o => o.RemainingQuantity).FirstOrDefault() + cutInDetail.PreparingQuantity,
                    }).ToList()
                }).ToList()
            }
            ).FirstOrDefault();

            await Task.Yield();
            return Ok(garmentCuttingOutDto);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PlaceGarmentCuttingOutCommand command)
        {
            try
            {
                VerifyUser();

                var order = await Mediator.Send(command);

                return Ok(order.Identity);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] UpdateGarmentCuttingOutCommand command)
        {
            Guid guid = Guid.Parse(id);

            command.SetIdentity(guid);

            VerifyUser();

            var order = await Mediator.Send(command);

            return Ok(order.Identity);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            Guid guid = Guid.Parse(id);

            VerifyUser();
            var usedData = false;
            var garmentSewingDO = _garmentSewingDORepository.Query.Where(o => o.CuttingOutId == guid).Select(o => new GarmentSewingDO(o)).Single();

            _garmentSewingDOItemRepository.Find(x => x.SewingDOId == garmentSewingDO.Identity).ForEach(async sewingDOItem =>
            {
                if (sewingDOItem.RemainingQuantity < sewingDOItem.Quantity)
                {
                    usedData = true;
                }
            });

            if(usedData == true)
            {
                return BadRequest(new
                {
                    code = HttpStatusCode.BadRequest,
                    error = "Data Sudah Digunakan di Sewing In"
                });
            } else
            {
                RemoveGarmentCuttingOutCommand command = new RemoveGarmentCuttingOutCommand(guid);
                var order = await Mediator.Send(command);

                return Ok(order.Identity);
            }
        }

		[HttpGet("monitoring")]
		public async Task<IActionResult> GetMonitoring(int unit, DateTime dateFrom, DateTime dateTo, int page = 1, int size = 25, string Order = "{}")
		{
			VerifyUser();
			GetMonitoringCuttingQuery query = new GetMonitoringCuttingQuery(page, size, Order, unit, dateFrom, dateTo, WorkContext.Token);
			var viewModel = await Mediator.Send(query);

			return Ok(viewModel.garmentMonitorings, info: new
			{
				page,
				size,
				viewModel.count
			});
		}
		[HttpGet("download")]
		public async Task<IActionResult> GetXls(int unit, DateTime dateFrom, DateTime dateTo, int page = 1, int size = 25, string Order = "{}")
		{
			try
			{
				VerifyUser();
				GetXlsCuttingQuery query = new GetXlsCuttingQuery(page, size, Order, unit, dateFrom, dateTo, WorkContext.Token);
				byte[] xlsInBytes;

				var xls = await Mediator.Send(query);

				string filename = "Laporan Cutting";

				if (dateFrom != null) filename += " " + ((DateTime)dateFrom).ToString("dd-MM-yyyy");

				if (dateTo != null) filename += "_" + ((DateTime)dateTo).ToString("dd-MM-yyyy");
				filename += ".xlsx";

				xlsInBytes = xls.ToArray();
				var file = File(xlsInBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
				return file;
			}
			catch (Exception e)
			{
				return StatusCode((int)HttpStatusCode.InternalServerError, e.Message);
			}
		}


	}
}