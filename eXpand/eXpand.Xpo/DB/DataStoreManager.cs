﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.Metadata;
using eXpand.Xpo.Attributes;
using System.Linq;

namespace eXpand.Xpo.DB {
    public class DataStoreManager {
        public const string STR_Default = "Default";
        readonly Dictionary<string,ReflectionDictionary> _reflectionDictionaries=new Dictionary<string, ReflectionDictionary>();
        readonly Dictionary<string,SimpleDataLayer> _simpleDataLayers=new Dictionary<string, SimpleDataLayer>();
        readonly Dictionary<string, List<string>> _tables=new Dictionary<string, List<string>>();
        readonly string _connectionString;
        readonly IEnumerable<DataStoreAttribute> _dataStoreAttributes;

        public DataStoreManager(string connectionString) {
            _connectionString = connectionString;
            _dataStoreAttributes = getDataStoreAttributes();
        }


        public string GetKey(Type type){
            var dataStoreAttribute = _dataStoreAttributes.Where(attribute => type.Namespace.StartsWith(attribute.NameSpace)).SingleOrDefault();
            return dataStoreAttribute == null ? STR_Default : dataStoreAttribute.DataStoreNameSuffix;
        }

        string GetKey(XPClassInfo xpClassInfo) {
            return GetKey(xpClassInfo.ClassType);
        }

        void AddTableNames(XPClassInfo xpClassInfo, string key) {
            List<string> list = _tables[key];
            list.Add(xpClassInfo.TableName);
            foreach (var tableName in GetIntermediateTableNames(xpClassInfo).Where(tableName => !list.Contains(tableName))) {
                list.Add(tableName);
            }
        }

        IEnumerable<string> GetIntermediateTableNames(XPClassInfo classInfo){
            return classInfo.CollectionProperties.OfType<XPMemberInfo>().Where(info => info.IntermediateClass != null).Select(memberInfo => memberInfo.IntermediateClass.TableName);
        }
        public ReflectionDictionary GetDictionary(Type type) {
            XPClassInfo xpClassInfo = GetXPClassInfo(type);
            return GetDictionary(xpClassInfo);
        }

        XPClassInfo GetXPClassInfo(Type type) {
            var xpClassInfos = _reflectionDictionaries.Select(pair => pair.Value).SelectMany(dictionary => dictionary.Classes.OfType<XPClassInfo>());
            return xpClassInfos.Where(info => info.ClassType==type).Single();
        }

        public ReflectionDictionary GetDictionary(XPClassInfo xpClassInfo)
        {
            string key = GetKey(xpClassInfo);
            var reflectionDictionary = GetDictionary(key);
            if (xpClassInfo.IsPersistent)
                AddTableNames(xpClassInfo, key);
            return reflectionDictionary;
        }

        ReflectionDictionary GetDictionary(string key) {
            if (!_reflectionDictionaries.ContainsKey(key)) {
                var reflectionDictionary = new ReflectionDictionary();
                _reflectionDictionaries.Add(key, reflectionDictionary);
                var simpleDataLayer = new SimpleDataLayer(reflectionDictionary,GetConnectionProvider(key));
                _simpleDataLayers.Add(key, simpleDataLayer);
                _tables.Add(key,new List<string>());
            }
            return _reflectionDictionaries[key];
        }

        public IDataStore GetConnectionProvider(Type type) {
            return GetConnectionProvider(GetKey(type));
        }

        public string GetConnectionString(Type type) {
            string key = GetKey(type);
            return GetConnectionString(key);
        }

        public string GetConnectionString(string key )
        {
            if (key==STR_Default)
                return _connectionString;
            ConnectionStringSettings connectionStringSettings =
                ConfigurationManager.ConnectionStrings[string.Format("{0}ConnectionString", key)];
            if (connectionStringSettings != null){
                return connectionStringSettings.ConnectionString;
            }
            IDataStore connectionProvider = XpoDefault.GetConnectionProvider(_connectionString, AutoCreateOption.DatabaseAndSchema);
            if (connectionProvider is ConnectionProviderSql)
            {
                IDbConnection dbConnection = ((ConnectionProviderSql)connectionProvider).Connection;
                return _connectionString == null ? AccessConnectionProvider.GetConnectionString(key)
                                              : _connectionString.Replace(dbConnection.Database, dbConnection.Database + key+".mdb");

            }
            throw new NoNullAllowedException(string.Format("{0}ConnectionString not found ", key));
        }

        public IDataStore GetConnectionProvider(string key){
            string connectionString = GetConnectionString(key);
            return XpoDefault.GetConnectionProvider(connectionString, AutoCreateOption.DatabaseAndSchema);
        }

        IEnumerable<DataStoreAttribute> getDataStoreAttributes() {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetCustomAttributes(typeof(DataStoreAttribute), false).OfType<DataStoreAttribute>());
        }

        public Dictionary<string, SimpleDataLayer> SimpleDataLayers {
            get { return _simpleDataLayers; }
        }

        public Dictionary<IDataStore, List<DBTable>> GetDataStores(DBTable[] dbTables){
            var dictionary = _simpleDataLayers.Select(pair => 
                                                      pair.Value.ConnectionProvider).ToDictionary(dataStore => dataStore, dataStore => new List<DBTable>());
            foreach (var dbTable in dbTables) {
                if (dbTable.Name == "XPObjectType")
                    foreach (var simpleDataLayer in _simpleDataLayers) {
                        dictionary[simpleDataLayer.Value.ConnectionProvider].Add(dbTable);
                    }
                else
                    try {
                        dictionary[_simpleDataLayers[GetKey(dbTable.Name)].ConnectionProvider].Add(dbTable);
                    }
                    catch (Exception e) {
                        Console.WriteLine(e);
                    }
            }
            return dictionary;
        }

        public string GetKey(string tableName) {
            var keyValuePairs = _tables.Where(valuePair => valuePair.Value.Contains(tableName));
            string key = STR_Default;
            if (keyValuePairs.Count() > 0)
                key=keyValuePairs.ToList()[0].Key;
            return key;
        }

        public Type GetType(string typeName) {
            var types = _reflectionDictionaries.Select(pair => pair.Value).SelectMany(dictionary => dictionary.Classes.OfType<XPClassInfo>()).Select(classInfo => classInfo.ClassType);
            return types.Where(type => type.Name==typeName).SingleOrDefault();
        }
    }
}