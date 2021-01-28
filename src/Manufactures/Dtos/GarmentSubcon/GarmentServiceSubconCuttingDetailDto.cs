﻿using Manufactures.Domain.GarmentSubcon.ServiceSubconCuttings;
using Manufactures.Domain.Shared.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manufactures.Dtos.GarmentSubcon
{
    public class GarmentServiceSubconCuttingDetailDto : BaseDto
    {
        public GarmentServiceSubconCuttingDetailDto(GarmentServiceSubconCuttingDetail garmentServiceSubconCuttingDetail)
        {
            Id = garmentServiceSubconCuttingDetail.Identity;
            ServiceSubconCuttingItemId = garmentServiceSubconCuttingDetail.ServiceSubconCuttingItemId;
            CuttingInDetailId = garmentServiceSubconCuttingDetail.CuttingInDetailId;
            Product = new Product(garmentServiceSubconCuttingDetail.ProductId.Value, garmentServiceSubconCuttingDetail.ProductCode, garmentServiceSubconCuttingDetail.ProductName);
            DesignColor = garmentServiceSubconCuttingDetail.DesignColor;
            Quantity = garmentServiceSubconCuttingDetail.Quantity;
        }

        public Guid Id { get; set; }
        public Guid ServiceSubconCuttingItemId { get; internal set; }
        public Guid CuttingInDetailId { get; internal set; }
        public Product Product { get; set; }

        public string DesignColor { get; set; }

        public double Quantity { get; set; }
    }
}