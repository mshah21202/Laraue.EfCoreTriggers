﻿using System;
using System.Linq.Expressions;
using Laraue.EfCoreTriggers.Common.Services.Impl.TriggerVisitors;
using Laraue.EfCoreTriggers.Common.SqlGeneration;
using Laraue.EfCoreTriggers.Common.TriggerBuilders.Base;
using Laraue.EfCoreTriggers.Common.TriggerBuilders.TableRefs;
using Laraue.EfCoreTriggers.Tests.Infrastructure;
using Xunit;
using Xunit.Categories;

namespace Laraue.EfCoreTriggers.Tests.Tests.Unit
{
    [UnitTest]
    public abstract class UnitRawSqlTests
    {
        protected readonly ITriggerActionVisitorFactory Factory;

        protected UnitRawSqlTests(ITriggerActionVisitorFactory factory)
        {
            Factory = factory;
        }
        
        protected abstract string ExceptedInsertTriggerSqlForMemberArgs { get; }
        
        [Fact]
        protected void GenerateSqlForMemberArgs()
        {
            Expression<Func<NewTableRef<SourceEntity>, object>> arg1Expression = sourceEntity => sourceEntity.New.BooleanValue;
            Expression<Func<NewTableRef<SourceEntity>, object>> arg2Expression = sourceEntity => sourceEntity.New.DoubleValue;
            Expression<Func<NewTableRef<SourceEntity>, object>> arg3Expression = sourceEntity => sourceEntity.New;
            
            var trigger = new TriggerRawAction<SourceEntity, NewTableRef<SourceEntity>>(
                "PERFORM func({0}, {1}, {2})",
                new [] { arg1Expression, arg2Expression, arg3Expression });

            var generatedSql = Factory.Visit(trigger, new VisitedMembers());

            Assert.Equal(ExceptedInsertTriggerSqlForMemberArgs, generatedSql);
        }
        
        
        protected abstract string ExceptedInsertTriggerSqlForComputedArgs { get; }
        
        [Fact]
        protected void GenerateSqlForComputedArgs()
        {
            Expression<Func<NewTableRef<SourceEntity>, object>> argExpression = sourceEntity
                => sourceEntity.New.DoubleValue + 10;
            
            var trigger = new TriggerRawAction<SourceEntity, NewTableRef<SourceEntity>>(
                "PERFORM func({0})", 
                new []{ argExpression });

            var generatedSql = Factory.Visit(trigger, new VisitedMembers());

            Assert.Equal(ExceptedInsertTriggerSqlForComputedArgs, generatedSql);
        }
        
        protected abstract string ExceptedInsertTriggerSqlWhenNoArgs { get; }
        
        [Fact]
        protected void GenerateSqlWhenNoArgs()
        {
            var trigger = new TriggerRawAction<SourceEntity, NewTableRef<SourceEntity>>("PERFORM func()");

            var generatedSql = Factory.Visit(trigger, new VisitedMembers());

            Assert.Equal(ExceptedInsertTriggerSqlWhenNoArgs, generatedSql);
        }
        
        protected abstract string ExceptedUpdateTriggerSqlForMemberArgs { get; }
        
        [Fact]
        protected void GenerateSqlForUpdateTrigger()
        {
            var trigger = new TriggerRawAction<SourceEntity, OldAndNewTableRefs<SourceEntity>>(
                "PERFORM func({0}, {1})",
                tableRefs => tableRefs.Old.DecimalValue,
                tableRefs => tableRefs.New.DecimalValue);

            var generatedSql = Factory.Visit(trigger, new VisitedMembers());

            Assert.Equal(ExceptedUpdateTriggerSqlForMemberArgs, generatedSql);
        }
        
        protected abstract string ExceptedDeleteTriggerSqlForMemberArgs { get; }
        
        [Fact]
        protected void GenerateSqlForDeleteTrigger()
        {
            var trigger = new TriggerRawAction<SourceEntity, OldTableRef<SourceEntity>>("PERFORM func({0}, {1})", 
                tableRefs => tableRefs.Old.DecimalValue, 
                tableRefs => tableRefs.Old.DoubleValue);

            var generatedSql = Factory.Visit(trigger, new VisitedMembers());

            Assert.Equal(ExceptedDeleteTriggerSqlForMemberArgs, generatedSql);
        }
    }
}