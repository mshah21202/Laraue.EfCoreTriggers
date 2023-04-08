﻿using System;
using System.Linq.Expressions;
using System.Reflection;
using Laraue.EfCoreTriggers.Common.SqlGeneration;
using Laraue.EfCoreTriggers.Common.TriggerBuilders;
using Laraue.EfCoreTriggers.Common.TriggerBuilders.TableRefs;

namespace Laraue.EfCoreTriggers.Common.Services.Impl.ExpressionVisitors;

/// <inheritdoc />
public class MemberExpressionVisitor : BaseExpressionVisitor<MemberExpression>
{
    private readonly ISqlGenerator _generator;
    
    /// <inheritdoc />
    public MemberExpressionVisitor(ISqlGenerator generator)
    {
        _generator = generator;
    }

    /// <inheritdoc />
    public override SqlBuilder Visit(MemberExpression expression, ArgumentTypes argumentTypes, VisitedMembers visitedMembers)
    {
        visitedMembers.AddMember(ArgumentType.Default, expression.Member);
        
        return SqlBuilder.FromString(Visit(expression, ArgumentType.Default));
    }
    
    /// <summary>
    /// Visit specified member with specified <see cref="ArgumentType"/>.
    /// </summary>
    /// <param name="memberExpression"></param>
    /// <param name="argumentType"></param>
    /// <returns></returns>
    protected virtual string Visit(MemberExpression memberExpression, ArgumentType argumentType)
    {
        if (memberExpression.Expression is MemberExpression nestedMemberExpression)
        {
            return TryGetTableRef(nestedMemberExpression, memberExpression.Member);
        }

        if (memberExpression.Member.TryGetNewTableRef(out _))
        {
            return _generator.NewEntityPrefix;
        }
        
        if (memberExpression.Member.TryGetOldTableRef(out _))
        {
            return _generator.OldEntityPrefix;
        }

        return _generator.GetColumnSql(
            memberExpression.Expression.Type,
            memberExpression.Member,
            argumentType);
    }

    private string TryGetTableRef(MemberExpression memberExpression, MemberInfo parentMember)
    {
        var argumentType = ArgumentType.Default;
        var memberType = memberExpression.Expression.Type;
        if (memberExpression.Member.TryGetNewTableRef(out var tableRefType))
        {
            memberType = tableRefType;
            argumentType = ArgumentType.New;
        }
        else if (memberExpression.Member.TryGetOldTableRef(out tableRefType))
        {
            memberType = tableRefType;
            argumentType = ArgumentType.Old;
        }
        
        // return bool + arg type
        
        
        // TODO - if memberExp.Exp is member exp,
        //  it is either json (not supported) or our case when new ref return NEW + get column name from the table ref.
        return _generator.GetColumnSql(
            memberType,
            parentMember,
            argumentType);
    }
}