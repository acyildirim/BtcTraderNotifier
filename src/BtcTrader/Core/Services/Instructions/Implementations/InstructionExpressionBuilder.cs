using BtcTrader.Domain.Instructions;
using BtcTrader.Infrastructure.Filters;
using BtcTrader.Infrastructure.Helpers;
using BtcTrader.Infrastructure.Services.Implementations;
using System.Linq.Expressions;

namespace BtcTrader.Core.Services.Instructions.Implementations;

public class InstructionExpressionBuilder : ExpressionBuilderService<Instruction>
{
    private const string ID = "id";
    private const string FILTER_BY_ID = "FilterBy.id";
    private const string USER_ID = "userId";
    private const string FILTER_BY_USER_ID = "FilterBy.userId";
    private const string INSTRUCTION_TYPE = "instructionType";
    private const string FILTER_BY_INSTRUCTION_TYPE = "FilterBy.instructionType";
    private const string NOTIFICATION_CHANNEL = "notificationChannel";
    private const string FILTER_BY_NOTIFICATION_CHANNEL = "FilterBy.notificationChannel";
    private const string IS_ACTIVE = "isActive";
    private const string FILTER_BY_IS_ACTIVE = "FilterBy.isActive";  
    private const string INSTRUCTION_DATE = "instructionDate";
    private const string FILTER_BY_INSTRUCTION_DATE = "FilterBy.instructionDate"; 
    protected override Expression<Func<Instruction, bool>> GetQueryExpression<Instruction>(Dictionary<string, string>? filterBy)
    {
        if (filterBy is null) return null;
        var queryFilters=
            filterBy.Select(keyValuePair => new QueryFilter(keyValuePair.Key, keyValuePair.Value, Operator.Equals)).ToList();
        for (var i = 0; i < queryFilters.Count; i++)
        {
            FigureOutFilterProperties(queryFilters[i]);
            if (queryFilters[i].PropertyName is null)
            {
                queryFilters.Remove(queryFilters[i]);
            }

        }
        var expression = QueryHelper.GetExpression<Instruction>(queryFilters);
        return expression;
    }
    private void FigureOutFilterProperties(QueryFilter queryFilter)
    {
        switch (queryFilter.PropertyName)
        {
            case ID:
            case FILTER_BY_ID:
                queryFilter.PropertyName = "Id";
                queryFilter.Operator = Operator.Equals;
                break;  
            case USER_ID:
            case FILTER_BY_USER_ID:
                queryFilter.PropertyName = "UserId";
                queryFilter.Operator = Operator.Equals;
                break; 
            case INSTRUCTION_TYPE:
            case FILTER_BY_INSTRUCTION_TYPE:
                queryFilter.PropertyName = "InstructionType";
                queryFilter.Operator = Operator.Contains;
                break;  
            case NOTIFICATION_CHANNEL:
            case FILTER_BY_NOTIFICATION_CHANNEL:
                queryFilter.PropertyName = "NotificationChannel";
                queryFilter.Operator = Operator.Contains;
                break; 
            case INSTRUCTION_DATE:
            case FILTER_BY_INSTRUCTION_DATE:
                queryFilter.PropertyName = "InstructionDate";
                queryFilter.Operator = Operator.Equals;
                break; 
            case IS_ACTIVE:
            case FILTER_BY_IS_ACTIVE:
                queryFilter.PropertyName = "IsActive";
                queryFilter.Operator = Operator.Equals;
                break;
            default:
                queryFilter.PropertyName = null;
                queryFilter.Operator = Operator.Equals;
                break;
        }
    }

   
}
