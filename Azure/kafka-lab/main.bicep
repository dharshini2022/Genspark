//Steps:
//1.set parameters
//2. create nsg
//3. create vnet
//4. create public ip
//5. create nic
//6. create vm
//7. create setup
//8. output public ip and SSH

//sets parameters
param location   string = 'southindia'
param vmName     string = 'dharshini-kafka-vm'
param adminUsername string = 'azureuser'
 
//marks password as sensitive info, so it can't be logged anywhere
//no default value is provided, so we will configure it during deployment
@secure()
param adminPassword string
 
//vmSize is the size of the VM, default is Standard_B2ms
//2 vCPUs and 8GB RAMs (2 vCPU = 2 CPU threads can be executed simultaneously at one processor)
param vmSize string = 'Standard_D2as_v5'
 
//sets variable names
//nsg is network security group => defines firewall
//vnet is virtual network (VPN)
var nsgName      = '${vmName}-nsg'
var vnetName     = '${vmName}-vnet'
var subnetName   = 'default'
var publicIpName = '${vmName}-pip'
var nicName      = '${vmName}-nic'
 
resource nsg 'Microsoft.Network/networkSecurityGroups@2023-04-01' = {
  name: nsgName
  location: location
  properties: {
    securityRules: [{
      name: 'AllowSSH'
      properties: {
        priority: 1000
        protocol: 'Tcp'
        access: 'Allow'
        //traffic comes from the internet to the VM
        direction: 'Inbound'
        //* => anywhere, any IP adress may attempt to connect
        sourceAddressPrefix: '*'
        sourcePortRange: '*'
        destinationAddressPrefix: '*'
        //The incoming traffic will be connected to this destination port
        destinationPortRange: '22'
      }
    }]
  }
}
 
resource vnet 'Microsoft.Network/virtualNetworks@2023-04-01' = {
  name: vnetName
  location: location
  properties: {
    //Enitre private network
    addressSpace: { addressPrefixes: ['10.0.0.0/16'] }
    //how the private network is divided into smaller parts (subnet)
    //external requests will access the subnet only
    subnets: [{
      name: subnetName
      properties: { addressPrefix: '10.0.0.0/24', networkSecurityGroup: { id: nsg.id } }
    }]
  }
}

//creates public IP so the VM is accesible via the internet
resource publicIp 'Microsoft.Network/publicIPAddresses@2023-04-01' = {
  name: publicIpName
  location: location
  sku: { name: 'Standard' }
  properties: { publicIPAllocationMethod: 'Static' }
}

//VM can't directly connect to subnet, so a Network Interface Card is used as a bridge
resource nic 'Microsoft.Network/networkInterfaces@2023-04-01' = {
  name: nicName
  location: location
  properties: {
    ipConfigurations: [{
      name: 'ipconfig1'
      properties: {
        subnet:          { id: '${vnet.id}/subnets/${subnetName}' }
        publicIPAddress: { id: publicIp.id }
      }
    }]
  }
}

//creating the resource : VM
resource vm 'Microsoft.Compute/virtualMachines@2023-07-01' = {
  name: vmName
  location: location
  properties: {
    hardwareProfile: { vmSize: vmSize }
    osProfile: {
      computerName:  vmName
      adminUsername: adminUsername
      adminPassword: adminPassword
    }
    storageProfile: {
      imageReference: {
        publisher: 'Canonical'
        offer:     '0001-com-ubuntu-server-jammy'
        sku:       '22_04-lts-gen2'
        version:   'latest'
      }
      osDisk: {
        createOption: 'FromImage'
        managedDisk:  { storageAccountType: 'StandardSSD_LRS' }
      }
    }
    networkProfile: { networkInterfaces: [{ id: nic.id }] }
  }
}
 
//Runs a post-deployment script to install and configure Kafka and its dependencies.
resource setup 'Microsoft.Compute/virtualMachines/extensions@2023-07-01' = {
  parent: vm
  name: 'setup-kafka'
  location: location
  properties: {
    publisher:            'Microsoft.Azure.Extensions'
    type:                 'CustomScript'
    typeHandlerVersion:   '2.1'
    autoUpgradeMinorVersion: true
    settings: {}
    protectedSettings: {
      script: base64(loadTextContent('setup.sh'))
    }
  }
}
 
//Returns the VM's public Ip after deployment
output publicIpAddress string = publicIp.properties.ipAddress
//Ready to use SSH command
output sshCommand      string = 'ssh ${adminUsername}@${publicIp.properties.ipAddress}'
