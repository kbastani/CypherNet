﻿namespace CypherNet
{
    #region

    using System;
    using System.Linq.Expressions;
    using Graph;
    using Queries;

    #endregion

    public interface ICypherSession
    {
        ICypherQueryStart<TVariables> BeginQuery<TVariables>();

        ICypherQueryStart<TVariables> BeginQuery<TVariables>(
            Expression<Func<ICypherPrototype, TVariables>> variablePrototype);

        Node CreateNode(object properties);
        Node CreateNode(object properties, string label);
        Node GetNode(long id);
        void Delete(Node node);
        void Delete(long nodeId);
        void Save(Node node);
    }
}