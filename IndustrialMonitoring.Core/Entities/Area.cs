using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrialMonitoring.Core.Enums;

namespace IndustrialMonitoring.Core.Entities
{
    public class Area
    {
        public Area(string name, string codArea, string description)
        {
            // Validation
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Nome da área é obrigatória", nameof(name));
            }

            if (string.IsNullOrEmpty(codArea))
            {
                throw new ArgumentException("Código da área é obrigatório", nameof(codArea));
            }

            Name = name;
            CodArea = codArea.ToUpper();
            Description = description ?? string.Empty;
            Status = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Automatic
        public int Id { get; private set; }

        public string Name { get; private set; }
        public string CodArea { get; private set; }
        public string Description { get; private set; }
        public bool Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<Equipment> _equipmentList = new();
        public IReadOnlyCollection<Equipment> EquipmentList => _equipmentList.AsReadOnly();


        public bool AddEquipment(Equipment equipment)
        {
            // Verifing if the equipment received is null or not
            if (equipment == null)
            {
                return false;
            }

            if (_equipmentList.Any(e => e.Id == equipment.Id))
            {
                return false;
            }

            // Deactivated area - It can not be used for a equipment
            if (!Status)
            {
                return false;
            }

            _equipmentList.Add(equipment);
            UpdateTimestamp();
            return true;
        }

        public bool RemoveEquipment(Equipment equipment)
        {
            if (equipment == null)
            {
                return false;
            }

            var removed = _equipmentList.Remove(equipment);
            if (removed)
            {
                UpdateTimestamp();
            }

            return removed;
        }

        public int GetTotalEquipments()
        {
            return _equipmentList.Count;
        }

        public int GetOperationalEquipments()
        {
            return _equipmentList.Count(eq => eq.Status == EquipmentStatus.Running);
        }

        public void UpdateDescription(string newDescription)
        {
            Description = newDescription ?? string.Empty;
            UpdateTimestamp();
        }

        public void Activate()
        {
            Status = true;
            UpdateTimestamp();
        }

        // Method used to deactive an area that is not operational anymore
        public void Deactivate()
        {
            if (GetTotalEquipments() == 0)
            {
                Status = false;
                UpdateTimestamp();
            }
            else
            {
                throw new InvalidOperationException("Não foi possível desativar a área com equipamentos operacionais");
            }
        }

        // Business validation
        public bool CanReceiveEquipment()
        {
            return Status;
        }

        public bool HasOperationalEquipments()
        {
            return GetOperationalEquipments() > 0;
        }

        private void UpdateTimestamp()
        {
            UpdatedAt = DateTime.UtcNow;
        }
        
        // Override to best debbuging
        public override string ToString()
        {
            return $"{CodArea} - {Name} ({GetTotalEquipments()} equipamentos)";
        }
    }
}