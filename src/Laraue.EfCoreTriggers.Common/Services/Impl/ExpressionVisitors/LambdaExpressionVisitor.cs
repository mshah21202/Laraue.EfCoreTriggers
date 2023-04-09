﻿using System.Linq.Expressions;
using Laraue.EfCoreTriggers.Common.SqlGeneration;
using Laraue.EfCoreTriggers.Common.TriggerBuilders;

namespace Laraue.EfCoreTriggers.Common.Services.Impl.ExpressionVisitors;

public class LambdaExpressionVisitor : BaseExpressionVisitor<LambdaExpression>
{
    private readonly IExpressionVisitorFactory _factory;

    public LambdaExpressionVisitor(IExpressionVisitorFactory factory)
    {
        _factory = factory;
    }

    public override SqlBuilder Visit(LambdaExpression expression, VisitedMembers visitedMembers)
    {
        return _factory.Visit(expression.Body, visitedMembers);
    }
}