using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IndustrialMonitoring.Core.Enums;

namespace IndustrialMonitoring.Core.Entities
{
    public class Equipment
    {
        public Equipment(string name, string serialNumber, EquipmentType type, string manufacturer, string model, DateTime installationDate, int areaId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Nome do Equipmaneto é obrigatório", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                throw new ArgumentException("Número de série é obrigatório", nameof(serialNumber));
            }

            if (string.IsNullOrWhiteSpace(manufacturer))
            {
                throw new ArgumentException("Fabricante é obrigatório", nameof(manufacturer));
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new ArgumentException("Modelo é obrigatório", nameof(model));
            }

            if (installationDate > DateTime.UtcNow)
            {
                throw new ArgumentException("Data de instalação não pode ser futura", nameof(installationDate));
            }

            if (areaId <= 0)
            {
                throw new ArgumentException("AreaId deve ser maior que zero", nameof(areaId));
            }

            Name = name;
            SerialNumber = serialNumber.ToUpper();
            Type = type;
            Manufacturer = manufacturer;
            Model = model;
            InstallationDate = installationDate;
            AreaId = areaId;

            Status = EquipmentStatus.Stopped;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;

        }

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string SerialNumber { get; private set; }
        public EquipmentType Type { get; private set; }
        public string Manufacturer { get; private set; }
        public string Model { get; private set; }
        public EquipmentStatus Status { get; private set; }
        public DateTime InstallationDate { get; private set; }
        public int AreaId { get; set; }
        public Area Area { get; set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private readonly List<Sensor> _sensors = new();
        private readonly List<MaintenanceRecord> _maintenanceRecords = new();

        public IReadOnlyCollection<Sensor> Sensors => _sensors.AsReadOnly();
        public IReadOnlyCollection<MaintenanceRecord> MaintenanceRecords => _maintenanceRecords.AsReadOnly();


        public bool AddSensor(Sensor sensor)
        {

        }

        public bool RemoveSensor(Sensor sensor)
        {

        }

        public void Start()
        {

        }

        public void Stop()
        {

        }

        public void SetMaintenance()
        {

        }

        public void SetFault()
        {

        }

        public int GetOperationalDays()
        {

        }

        public DateTime? GetNextMaintenanceDate()
        {

        }

        // Consultas de Estado
        public bool IsOperational()
        {

        }

        public bool HasActiveSensors()
        {

        }

        public bool NeedsMaintenance()
        {

        }

        public int GetMaintenanceCount()
        {

        }

        // Validações de negócio
        public bool CanStart()
        {

        }

        public bool CanReceiveMaintenance()
        {
            
        }
    }
}