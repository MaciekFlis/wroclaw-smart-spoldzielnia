module Service
open Building

type ServiceId = ServiceId of int

type ServiceDto = 
  {
    Id : int;
    Name : string;
    Description : string;
  }

type Service = 
  {
    Id : ServiceId;
    Building : Building;
    Name : string;
    Description : string;
  }

type ServicesError = 
  | ServiceNotFound
  | BuildingNotFound
  | Consistency
  | Panic

type RetrievedServices = Result<Service list, ServicesError>
type ServicesRepository = BuildingId -> ServiceDto seq option

type GetServicesForBuilding = BuildingId -> RetrievedServices

let getServicesForBuilding 
    : (GetBuilding -> ServicesRepository
      -> GetServicesForBuilding) = 
    let toService building (dto : ServiceDto) = 
      {
        Id = ServiceId(dto.Id);
        Building = building;
        Name = dto.Name;
        Description = dto.Description
      }
    fun buildingDomainService servicesRepo buildingId -> 
      match buildingDomainService buildingId with
      | Ok(building) -> 
        match servicesRepo buildingId with
        | None -> Error Panic
        | Some serviceDtos -> Ok (serviceDtos |> Seq.map (toService building) |> List.ofSeq)
      | Error NotFound -> Error BuildingNotFound
      | Error Building.Panic -> Error Panic
      | Error FoundDuplicate -> Error Consistency
