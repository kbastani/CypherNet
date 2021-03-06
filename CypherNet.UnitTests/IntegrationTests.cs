﻿namespace CypherNet.UnitTests
{
    #region

    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Transactions;
    using Configuration;
    using Graph;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Queries;
    using Transaction;

    #endregion

    [TestClass]
    public class IntegrationTests
    {
        private static Node _personNode, _positionNode;

        [TestMethod]
        public void CreateNode_ReturnsNewNode()
        {
            var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
            var endpoint = clientFactory.Create();

            _personNode = endpoint.CreateNode(new {name = "mark", age = 33}, "person");
            var twin = endpoint.GetNode(_personNode.Id);

            Assert.AreEqual(twin.Id, _personNode.Id);
        }


        [TestMethod]
        public void DeleteNode_DeletesNode()
        {
            var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
            var endpoint = clientFactory.Create();

            var node  = endpoint.CreateNode(new { name = "mark", age = 33 }, "person");
            endpoint.Delete(node);
            var twin = endpoint.GetNode(node.Id);

            Assert.IsNull(twin);
        }

        [TestMethod]
        public void UpdateNode_UpdatesNode()
        {
            var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
            var endpoint = clientFactory.Create();

            dynamic node =  endpoint.CreateNode(new { name = "mark", age = 33 }, "person");
            node.name = "john";
            endpoint.Save(node);
            dynamic twin = endpoint.GetNode(node.Id);

            Assert.AreEqual(twin.name, "john");
        }

        [TestMethod]
        public void CreateNode_WithLabel_ReturnsNewNode()
        {
            var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
            var endpoint = clientFactory.Create();

            _personNode = endpoint.CreateNode(new {name = "mark", age = 33}, "person");
            dynamic node = _personNode;

            Assert.AreEqual(node.name, "mark");
            Assert.AreEqual(node.age, 33);
        }

        [TestMethod]
        public void CreateNode_WithoutLabel_ReturnsNewNode()
        {
            var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
            var endpoint = clientFactory.Create();

            _positionNode = endpoint.CreateNode(new {position = "developer"});

            var newnode = endpoint
                .BeginQuery(s => new {n = s.Node})
                .Start(v => Start.At(v.n, _positionNode.Id))
                .Return(r => new {NewNode = r.n})
                .Fetch().Select(s => s.NewNode).FirstOrDefault();

            Assert.IsNotNull(newnode);
            Assert.IsTrue(_positionNode.Id == newnode.Id);
        }

        [TestMethod]
        public void CreateRelationship_ReturnsResults()
        {
            var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
            var endpoint = clientFactory.Create();

            var path = endpoint
                .BeginQuery(s => new {person = s.Node, worksAs = s.Rel, position = s.Node})
                .Start(v => Start.At(v.person, _personNode.Id).At(v.position, _positionNode.Id))
                .Create(v => Create.Relationship(v.person, v.worksAs, "WORKS_AS", v.position))
                .Return(s => new {s.person, s.worksAs, s.position})
                .Fetch().FirstOrDefault();

            Assert.IsNotNull(path);
            Assert.AreEqual("mark WORKS_AS developer",
                            String.Format("{0} {1} {2}", path.person.Get<string>("name"), path.worksAs.Type,
                                          path.position.Get<string>("position")));
        }


        [TestMethod]
        public void CreateNodeWithinTransaction_Rollback_DoesNotCreateNode()
        {
            var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
            Node node = null;

            using (var trans = new TransactionScope())
            {
                var endpoint = clientFactory.Create();
                node = endpoint.CreateNode(new {name = "mark", age = 33});
            }

            var readEndpoint = clientFactory.Create();
            var newnode = readEndpoint.BeginQuery(s => new {n = s.Node})
                                      .Start(v => Start.At(v.n, node.Id))
                                      .Return(r => new {NewNode = r.n})
                                      .Fetch().FirstOrDefault();

            Assert.IsNull(newnode);
        }

        [TestMethod]
        public void QueryGraph_SimpleQueryNotInsideTransaction_ReturnsResults()
        {
            var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
            var endpoint = clientFactory.Create();

            var nodes = endpoint.BeginQuery(p => new {node = p.Node})
                                .Start(n => Start.At(n.node, _personNode.Id))
                                .Return(r => new {Node = r.node})
                                .Fetch();

            Assert.AreEqual(nodes.Count(), 1);
            Assert.AreEqual(nodes.First().Node.Id, _personNode.Id);
        }


        [TestMethod]
        public void QueryWithJoinsOverMany_NotInsideTransaction_ReturnsMultipleResults()
        {
            var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
            var cypherEndpoint = clientFactory.Create();

            var nodes = cypherEndpoint
                .BeginQuery(p => new {person = p.Node, rel = p.Rel, role = p.Node}) // Define query variables
                .Start(vars => Start.Any(vars.person)) // Cypher START clause
                .Match(vars => Pattern.Start(vars.person).Outgoing(vars.rel).To(vars.role)) // Cypher MATCH clause
                .Where(
                       vars =>
                       vars.person.Get<string>("name!") == "mark" && vars.role.Get<string>("title!") == "developer")
                // Cypher WHERE predicate
                .Return(vars => new {Person = vars.person, Rel = vars.rel, Role = vars.role}) // Cypher RETURN clause
                .Fetch(); // GO!

            Assert.IsTrue(nodes.Any());

            foreach (var node in nodes)
            {
                dynamic start = node.Person; // Nodes & Relationships are dynamic types
                dynamic end = node.Role;
                Assert.AreEqual("mark", start.name);
                Assert.AreEqual("developer", end.title);
                Console.WriteLine(String.Format("{0} {1} {2}", start.name, node.Rel.Type, end.title));
                    // Prints "mark IS_A developer"
            }
        }

        [TestMethod]
        public void QueryGraph_SimpleQueryInsideTransaction_ReturnsResults()
        {
            using (var trans = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromDays(1)))
            {
                var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
                var cypherEndpoint = clientFactory.Create();
                var nodes = cypherEndpoint.BeginQuery(p => new {node = p.Node})
                                          .Start(n => Start.Any(n.node))
                                          .Where(n => n.node.Id == _personNode.Id)
                                          .Return(r => new {Node = r.node})
                                          .Fetch();
                Assert.AreEqual(nodes.Count(), 1);
                trans.Complete();
            }
        }

        [TestMethod]
        public void NestedTransactions_CommitInnerRollbackOuter_DoesNotCreateOuterNode()
        {
            var clientFactory = Fluently.Configure("http://localhost:7474/db/data/").CreateSessionFactory();
            var cypherEndpoint = clientFactory.Create();
            Node node1, node2;
            using (var trans1 = new TransactionScope(TransactionScopeOption.RequiresNew, TimeSpan.FromDays(1)))
            {
                node1 = cypherEndpoint.CreateNode(new {name = "test node1"});
                using (var trans2 = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    node2 = cypherEndpoint.CreateNode(new {name = "test node2"});
                    trans2.Complete();
                }
            }

            var node1Query = cypherEndpoint.BeginQuery(s => new {node1 = s.Node})
                                           .Start(s => Start.At(s.node1, node1.Id))
                                           .Return(r => new {r.node1})
                                           .Fetch()
                                           .FirstOrDefault();

            var node2Query = cypherEndpoint.BeginQuery(s => new {node2 = s.Node})
                                           .Start(s => Start.At(s.node2, node2.Id))
                                           .Return(r => new {r.node2})
                                           .Fetch()
                                           .FirstOrDefault();

            Assert.IsNull(node1Query);
            Assert.IsNotNull(node2Query);
        }

        internal class TestDoSOmething<TTemplate>
        {
            internal void DoSomething<TInterface>(Expression<Action<TInterface>> func)
                where TInterface : TTemplate, ICypherClientFactory
            {
            }
        }
    }
}