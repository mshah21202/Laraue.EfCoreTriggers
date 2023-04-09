﻿using System;
using Laraue.EfCoreTriggers.Common.SqlGeneration;
using Laraue.EfCoreTriggers.Common.TriggerBuilders;
using Laraue.EfCoreTriggers.Common.TriggerBuilders.Actions;
using Laraue.EfCoreTriggers.Common.TriggerBuilders.TableRefs;
using Laraue.EfCoreTriggers.Common.Visitors.TriggerVisitors;
using Laraue.EfCoreTriggers.MySql.Extensions;
using Laraue.EfCoreTriggers.Tests.Entities;
using Laraue.EfCoreTriggers.Tests.Enums;
using Laraue.EfCoreTriggers.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Categories;

namespace Laraue.EfCoreTriggers.Tests.Tests
{
    [UnitTest]
	public class ConditionGeneratingTests
    {
        private readonly ITriggerActionVisitorFactory _provider;

        public ConditionGeneratingTests()
        {
            var modelBuilder = new ModelBuilder();
            modelBuilder.Entity<User>()
                .Property<UserRole>("Role");

            modelBuilder.Entity<TestEntity>()
                .Property<char>("CharValue");

            modelBuilder.Entity<TestEntity>()
                .Property<UserRole>("EnumValue")
                .HasColumnType("varchar(100)")
                .HasConversion<string>();

            modelBuilder.Entity<TestEntity>()
                .Property<string>("StringValue");
            
            modelBuilder.Entity<TestEntity>()
                .Property<int?>("NullableIntValue");

            _provider = Helper.GetTriggerActionFactory(modelBuilder.Model.FinalizeModel(), collection => collection.AddMySqlServices());
        }

        [Fact]
        public void CastingToSameTypeShouldBeIgnored()
        {
            var action = new TriggerCondition<User, NewTableRef<User>>(
                tableRefs => tableRefs.New.Role > UserRole.Admin);
            var sql = _provider.Visit(action, new VisitedMembers());
            Assert.Equal("NEW.`Role` > 999", sql);
        }

        [Fact]
        public void CastingToAnotherTypeShouldBeTranslated()
        {
            var action = new TriggerCondition<User, NewTableRef<User>>(
                tableRefs => (decimal)tableRefs.New.Role > 50m);
            var sql = _provider.Visit(action, new VisitedMembers());
            Assert.Equal("CAST(NEW.`Role` AS DECIMAL) > 50", sql);
        }
        
        [Fact]
        public void ComparisonWithCharTypeShouldProduceCorrectSql()
        {
            var action = new TriggerCondition<TestEntity, NewTableRef<TestEntity>>(
                tableRefs => tableRefs.New.CharValue == 'D');
            var sql = _provider.Visit(action, new VisitedMembers());
            Assert.Equal("NEW.`CharValue` = 'D'", sql);
        }
        
        [Fact]
        public void StringEnumShouldGenerateCorrectSql()
        {
            var action = new TriggerCondition<TestEntity, NewTableRef<TestEntity>>(
                tableRefs => tableRefs.New.EnumValue == UserRole.Admin);
            var sql = _provider.Visit(action, new VisitedMembers());
            Assert.Equal("NEW.`EnumValue` = 'Admin'", sql);
        }
        
        [Fact]
        public void OnlyNotConstantConditionsShouldBeAddedToTrigger()
        {
            Assert.Throws<InvalidOperationException>(() => 
                new Trigger<User, OldAndNewTableRefs<User>>(TriggerEvent.Update, TriggerTime.After)
                    .Action(actions => actions
                        .Condition(_ => true)
                        .Insert(_ => new User { Role = UserRole.Usual })));
        }
        
        [Fact]
        public void NotEqualToNullShouldGenerateCorrectSql()
        {
            var action = new TriggerCondition<TestEntity, NewTableRef<TestEntity>>(
                tableRefs => tableRefs.New.StringValue != null);
            var sql = _provider.Visit(action, new VisitedMembers());
            Assert.Equal("NEW.`StringValue` IS NOT NULL", sql);
        }
        
        [Fact]
        public void EqualToNullShouldGenerateCorrectSql()
        {
            var action = new TriggerCondition<TestEntity, NewTableRef<TestEntity>>(
                tableRefs => tableRefs.New.StringValue == null);
            var sql = _provider.Visit(action, new VisitedMembers());
            Assert.Equal("NEW.`StringValue` IS NULL", sql);
        }
        
        [Fact]
        public void СoalesceNullableStructShouldGenerateCorrectSql()
        {
            var action = new TriggerCondition<TestEntity, NewTableRef<TestEntity>>(
                tableRefs => (tableRefs.New.NullableIntValue ?? 0) != 1);
            var sql = _provider.Visit(action, new VisitedMembers());
            Assert.Equal("COALESCE(NEW.`NullableIntValue`, 0) <> 1", sql);
        }
    }
}