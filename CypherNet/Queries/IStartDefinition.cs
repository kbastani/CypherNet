﻿namespace CypherNet.Queries
{
    #region

    using System;
    using Graph;

    #endregion

    public interface ICypherPrototype
    {
        Node Node { get; }
        Relationship Rel { get; }
    }

    public class Start
    {
        [ParseToCypher("{0}=node({1})")]
        public static IBeginRelationshipDefinition At(
            [ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Node nodeRef, long id)
        {
            throw new NotImplementedException();
        }

        [ParseToCypher("{0}=node:{1}({2})")]
        public static IBeginRelationshipDefinition At(
            [ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Node nodeRef, string index,
            string query)
        {
            throw new NotImplementedException();
        }

        [ParseToCypher("{0}=relationship({1})")]
        public static IBeginRelationshipDefinition At(
            [ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Relationship relationshipReference,
            long id)
        {
            throw new NotImplementedException();
        }

        [ParseToCypher("{0}=relationship:{1}({2})")]
        public static IBeginRelationshipDefinition At(
            [ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Relationship relationshipReference,
            string index, string query)
        {
            throw new NotImplementedException();
        }

        [ParseToCypher("{0}=node(*)")]
        public static IBeginRelationshipDefinition Any(
            [ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Node nodeRe)
        {
            throw new NotImplementedException();
        }

        [ParseToCypher("{0}=relationship(*)")]
        public static IBeginRelationshipDefinition Any(
            [ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Relationship nodeRe)
        {
            throw new NotImplementedException();
        }
    }

    public interface IBeginRelationshipDefinition
    {
        [ParseToCypher("{0}=node({1})")]
        void At([ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Node nodeRef, long id);

        [ParseToCypher("{0}=node:{1}({2})")]
        void At([ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Node nodeRef, string index,
                string query);

        [ParseToCypher("{0}=relationship({1})")]
        void At(
            [ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Relationship relationshipReference,
            long id);

        [ParseToCypher("{0}=relationship:{1}({2})")]
        void At(
            [ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Relationship relationshipReference,
            string index, string query);

        [ParseToCypher("{0}=node(*)")]
        void Any([ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Node nodeRe);

        [ParseToCypher("{0}=relationship(*)")]
        void Any(
            [ArgumentEvaluator(typeof (MemberNameArgumentEvaluator))] Relationship nodeRe);
    }
}