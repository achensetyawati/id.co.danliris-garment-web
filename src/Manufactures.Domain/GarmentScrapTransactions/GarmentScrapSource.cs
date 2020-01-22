﻿using Infrastructure.Domain;
using Manufactures.Domain.Events;
using Manufactures.Domain.GarmentScrapTransactions.ReadModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Manufactures.Domain.GarmentScrapTransactions
{
	public class GarmentScrapSource : AggregateRoot<GarmentScrapSource, GarmentScrapSourceReadModel>
	{

		public string Code { get; private set; }
		public string Name { get; private set; }
		public string Description { get; private set; }

		protected override GarmentScrapSource GetEntity()
		{
			return this;
		}
		public GarmentScrapSource(Guid identity, string code, string name, string description) : base(identity)
		{
			Identity = identity;
			Code = code;
			Name = name;
			Description = description;
			ReadModel = new GarmentScrapSourceReadModel(Identity)
			{
				Code = code,
				Name = name,
				Description = description,
			};

			ReadModel.AddDomainEvent(new OnGarmentScrapSourcePlaced(Identity));
		}
		public GarmentScrapSource(GarmentScrapSourceReadModel readModel) : base(readModel)
		{
			Code = readModel.Code;
			Name = readModel.Name;
			Description = readModel.Description;
		}
	}
}