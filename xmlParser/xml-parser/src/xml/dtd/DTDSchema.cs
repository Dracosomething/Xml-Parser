using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xml_parser.src.xml.dtd;

namespace xml_parser.src.xml
{
    internal class DTDSchema
    {
        private List<DTDAttList> attributeLists;
        private List<DTDElement> elements;
        private List<DTDEntity> entities;
        private List<DTDNotation> notations;
        private List<DTDEntity> buildInEntities;

        public DTDSchema()
        {
            attributeLists = new List<DTDAttList>();
            elements = new List<DTDElement>();
            entities = new List<DTDEntity>();
            notations = new List<DTDNotation>();
            buildInEntities = new List<DTDEntity> { 
                new DTDEntity("lt", "<", true), 
                new DTDEntity("gt", ">", true), 
                new DTDEntity("quot", "\"", true),
                new DTDEntity("apos", "'", true),
                new DTDEntity("amp", "&", true)};
        }

        public void addAtrributeList(DTDAttList list)
        {
            this.attributeLists.Add(list);
        }

        public void addElement(DTDElement element)
        {
            this.elements.Add(element);
        }

        public void addEntity(DTDEntity entity)
        {
            this.entities.Add(entity);
        }

        public void addNotation(DTDNotation notation)
        {
            this.notations.Add(notation);
        }

        public DTDEntity getEntity(string name, bool inDTDFile)
        {
            return this.entities.Find((entity) =>
            {
                if (!inDTDFile)
                    return entity.IsGlobal && entity.Name == name;
                return entity.Name == name;
            });
        }
        
        public DTDEntity getBuildInEntity(string name)
        {
            return this.buildInEntities.Find((entity) => entity.Name == name);
        }

        public void Combine(DTDSchema other)
        {
            this.attributeLists.AddRange(other.attributeLists);
            this.elements.AddRange(other.elements);
            this.entities.AddRange(other.entities);
            this.notations.AddRange(other.notations);
        }
    }
}
