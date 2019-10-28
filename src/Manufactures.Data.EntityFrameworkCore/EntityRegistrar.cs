﻿// Copyright © 2017 Dmitry Sikorsky. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using ExtCore.Data.EntityFramework;
using Manufactures.Data.EntityFrameworkCore.GarmentAvalProducts.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentCuttingIns.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentCuttingOuts.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentDeliveryReturns.Config;
using Manufactures.Data.EntityFrameworkCore.GarmentLoadings.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentPreparings.Config;
using Manufactures.Data.EntityFrameworkCore.GarmentSewingDOs.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentSewingIns.Configs;
using Manufactures.Data.EntityFrameworkCore.GarmentSewingOuts.Configs;
using Manufactures.Domain.GarmentAvalProducts.ReadModels;
using Manufactures.Domain.GarmentCuttingIns.ReadModels;
using Manufactures.Domain.GarmentCuttingOuts.ReadModels;
using Manufactures.Domain.GarmentDeliveryReturns.ReadModels;
using Manufactures.Domain.GarmentPreparings.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace Manufactures.Data.EntityFrameworkCore
{
    public class EntityRegistrar : IEntityRegistrar
    {
        public void RegisterEntities(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new GarmentPreparingConfig());
            modelBuilder.ApplyConfiguration(new GarmentPreparingItemConfig());

            modelBuilder.ApplyConfiguration(new GarmentAvalProductConfig());
            modelBuilder.ApplyConfiguration(new GarmentAvalProductItemConfig());

            modelBuilder.ApplyConfiguration(new GarmentCuttingInConfig());
            modelBuilder.ApplyConfiguration(new GarmentCuttingInItemConfig());
            modelBuilder.ApplyConfiguration(new GarmentCuttingInDetailConfig());

            modelBuilder.ApplyConfiguration(new GarmentDeliveryReturnConfig());
            modelBuilder.ApplyConfiguration(new GarmentDeliveryReturnItemConfig());

            modelBuilder.ApplyConfiguration(new GarmentCuttingOutConfig());
            modelBuilder.ApplyConfiguration(new GarmentCuttingOutItemConfig());
            modelBuilder.ApplyConfiguration(new GarmentCuttingOutDetailConfig());

            modelBuilder.ApplyConfiguration(new GarmentSewingDOConfig());
            modelBuilder.ApplyConfiguration(new GarmentSewingDOItemConfig());

            modelBuilder.ApplyConfiguration(new GarmentLoadingConfig());
            modelBuilder.ApplyConfiguration(new GarmentLoadingItemConfig());

            modelBuilder.ApplyConfiguration(new GarmentSewingInConfig());
            modelBuilder.ApplyConfiguration(new GarmentSewingInItemConfig());

            modelBuilder.ApplyConfiguration(new GarmentSewingOutConfig());
            modelBuilder.ApplyConfiguration(new GarmentSewingOutItemConfig());
            modelBuilder.ApplyConfiguration(new GarmentSewingOutDetailConfig());
        }
    }
}
