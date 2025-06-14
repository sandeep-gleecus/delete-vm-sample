﻿// Copyright (c) Microsoft Corporation.  All rights reserved.

using System.Data.Common;

namespace EFProviderWrapperToolkit
{
    /// <summary>
    /// Implementation of common methods for <see cref="DbTransaction"/> wrappers.
    /// </summary>
    public abstract class DBTransactionWrapper : DbTransaction
    {
        private DbTransaction wrappedTransaction;
        private DBConnectionWrapper connectionWrapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="DBTransactionWrapper"/> class.
        /// </summary>
        /// <param name="wrappedTransaction">The wrapped transaction.</param>
        /// <param name="connection">The connection.</param>
        protected DBTransactionWrapper(DbTransaction wrappedTransaction, DBConnectionWrapper connection)
        {
            this.wrappedTransaction = wrappedTransaction;
            this.connectionWrapper = connection;
        }

        /// <summary>
        /// Specifies the <see cref="T:System.Data.IsolationLevel"/> for this transaction.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The <see cref="T:System.Data.IsolationLevel"/> for this transaction.
        /// </returns>
        public override System.Data.IsolationLevel IsolationLevel
        {
            get { return this.wrappedTransaction.IsolationLevel; }
        }

        /// <summary>
        /// Gets the wrapped transaction.
        /// </summary>
        /// <value>The wrapped transaction.</value>
        public DbTransaction WrappedTransaction
        {
            get { return this.wrappedTransaction; }
        }

        /// <summary>
        /// Specifies the <see cref="T:System.Data.Common.DbConnection"/> object associated with the transaction.
        /// </summary>
        /// <value></value>
        /// <returns>
        /// The <see cref="T:System.Data.Common.DbConnection"/> object associated with the transaction.
        /// </returns>
        protected override DbConnection DbConnection
        {
            get { return this.connectionWrapper; }
        }

        /// <summary>
        /// Gets the connection wrapper.
        /// </summary>
        /// <value>The connection wrapper.</value>
        protected DBConnectionWrapper ConnectionWrapper
        {
            get { return this.connectionWrapper; }
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        public override void Commit()
        {
            this.wrappedTransaction.Commit();
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        public override void Rollback()
        {
            this.wrappedTransaction.Rollback();
        }
    }
}
