﻿using ExtCore.Data.Abstractions;
using Infrastructure.Domain.Commands;
using Manufactures.Domain.Shared.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Manufactures.Domain.GarmentSewingOuts;
using Manufactures.Domain.GermentReciptSubcon.GarmentFinishingIns.Commands;
using Manufactures.Domain.GermentReciptSubcon.GarmentFinishingIns;
using Manufactures.Domain.GermentReciptSubcon.GarmentFinishingIns.Repositories;
using Manufactures.Domain.GermentReciptSubcon.GarmentSewingOuts.Repositories;
using Manufactures.Domain.GermentReciptSubcon.GarmentSewingOuts;
using Manufactures.Domain.GermentReciptSubcon.GarmentSewingIns.Repositories;
using Manufactures.Domain.GermentReciptSubcon.GarmentSewingIns;

namespace Manufactures.Application.GermentReciptSubcon.GarmentFinishingIns.CommandHandlers
{
    public class PlaceGarmentSubconFinishingInCommandHandler : ICommandHandler<PlaceGarmentSubconFinishingInCommand, GarmentSubconFinishingIn>
    {
        private readonly IStorage _storage;
        private readonly IGarmentSubconFinishingInRepository _garmentFinishingInRepository;
        private readonly IGarmentSubconFinishingInItemRepository _garmentFinishingInItemRepository;
        private readonly IGarmentSubconSewingOutItemRepository _garmentSewingOutItemRepository;
        private readonly IGarmentSubconSewingInItemRepository _garmentSewingInItemRepository;

        public PlaceGarmentSubconFinishingInCommandHandler(IStorage storage)
        {
            _storage = storage;
            _garmentFinishingInRepository = storage.GetRepository<IGarmentSubconFinishingInRepository>();
            _garmentFinishingInItemRepository = storage.GetRepository<IGarmentSubconFinishingInItemRepository>();
            _garmentSewingOutItemRepository = storage.GetRepository<IGarmentSubconSewingOutItemRepository>();
            _garmentSewingInItemRepository = storage.GetRepository<IGarmentSubconSewingInItemRepository>();
        }

        public async Task<GarmentSubconFinishingIn> Handle(PlaceGarmentSubconFinishingInCommand request, CancellationToken cancellationToken)
        {
            request.Items = request.Items.ToList();

            GarmentSubconFinishingIn garmentFinishingIn = new GarmentSubconFinishingIn(
                Guid.NewGuid(),
                GenerateFinishingInNo(request),
                request.FinishingInType,
                new UnitDepartmentId(request.UnitFrom.Id),
                request.UnitFrom.Code,
                request.UnitFrom.Name,
                request.RONo,
                request.Article,
                new UnitDepartmentId(request.Unit.Id),
                request.Unit.Code,
                request.Unit.Name,
                request.FinishingInDate.GetValueOrDefault(),
                new GarmentComodityId(request.Comodity.Id),
                request.Comodity.Code,
                request.Comodity.Name,
                request.DOId,
                request.DONo,
                request.SubconType,
                false
            );

            Dictionary<Guid, double> sewingOutItemToBeUpdated = new Dictionary<Guid, double>();

            foreach (var item in request.Items)
            {
                if (item.IsSave)
                {
                    GarmentSubconFinishingInItem garmentFinishingInItem = new GarmentSubconFinishingInItem(
                    Guid.NewGuid(),
                    garmentFinishingIn.Identity,
                    item.SewingOutItemId,
                    item.SewingOutDetailId,
                    Guid.Empty,
                    new SizeId(item.Size.Id),
                    item.Size.Size,
                    new ProductId(item.Product.Id),
                    item.Product.Code,
                    item.Product.Name,
                    item.DesignColor,
                    item.Quantity,
                    item.RemainingQuantity,
                    new UomId(item.Uom.Id),
                    item.Uom.Unit,
                    item.Color,
                    item.BasicPrice,
                    item.Price
                    );

                    if (sewingOutItemToBeUpdated.ContainsKey(item.SewingOutItemId))
                    {
                        sewingOutItemToBeUpdated[item.SewingOutItemId] += item.Quantity;
                    }
                    else
                    {
                        sewingOutItemToBeUpdated.Add(item.SewingOutItemId, item.Quantity);
                    }

                    await _garmentFinishingInItemRepository.Update(garmentFinishingInItem);
                }
                else
                {
                    //Remove sewing out when data Item not selected
                    var garmentSewingOutItem = _garmentSewingOutItemRepository.Query.Where(o => o.Identity == item.SewingOutItemId).Select(s => new GarmentSubconSewingOutItem(s)).Single();

                    double diffQty = garmentSewingOutItem.Quantity;

                    if (diffQty > 0)
                    {
                        var garmentSewingInItem = _garmentSewingInItemRepository.Query.Where(x => x.Identity == garmentSewingOutItem.SewingInItemId).Select(s => new GarmentSubconSewingInItem(s)).Single();

                        garmentSewingInItem.SetRemainingQuantity(garmentSewingInItem.RemainingQuantity + diffQty);

                        garmentSewingInItem.Modify();

                        await _garmentSewingInItemRepository.Update(garmentSewingInItem);
                    }

                    garmentSewingOutItem.Remove();

                    await _garmentSewingOutItemRepository.Update(garmentSewingOutItem);
                }
            }

            foreach (var sewingDOItem in sewingOutItemToBeUpdated)
            {
                var garmentSewingOutItem = _garmentSewingOutItemRepository.Query.Where(x => x.Identity == sewingDOItem.Key).Select(s => new GarmentSubconSewingOutItem(s)).Single();
                
                double diffQty = garmentSewingOutItem.Quantity - sewingDOItem.Value;
                garmentSewingOutItem.SetRealQtyOut(sewingDOItem.Value);
                garmentSewingOutItem.Modify();

                await _garmentSewingOutItemRepository.Update(garmentSewingOutItem);

                if (diffQty > 0)
                {
                    var garmentSewingInItem = _garmentSewingInItemRepository.Query.Where(x => x.Identity == garmentSewingOutItem.SewingInItemId).Select(s => new GarmentSubconSewingInItem(s)).Single();

                    garmentSewingInItem.SetRemainingQuantity(garmentSewingInItem.RemainingQuantity + diffQty);

                    garmentSewingInItem.Modify();

                    await _garmentSewingInItemRepository.Update(garmentSewingInItem);
                }
            }

            await _garmentFinishingInRepository.Update(garmentFinishingIn);

            _storage.Save();

            return garmentFinishingIn;
        }

        private string GenerateFinishingInNo(PlaceGarmentSubconFinishingInCommand request)
        {
            var now = DateTime.Now;
            var year = now.ToString("yy");
            var month = now.ToString("MM");
            var day = now.ToString("dd");
            var unitcode = request.Unit.Code;

            var prefix = $"FI{unitcode}{year}{month}";

            var lastFinishingInNo = _garmentFinishingInRepository.Query.Where(w => w.FinishingInNo.StartsWith(prefix))
                .OrderByDescending(o => o.FinishingInNo)
                .Select(s => int.Parse(s.FinishingInNo.Replace(prefix, "")))
                .FirstOrDefault();
            var finInNo = $"{prefix}{(lastFinishingInNo + 1).ToString("D4")}";

            return finInNo;
        }
    }
}
