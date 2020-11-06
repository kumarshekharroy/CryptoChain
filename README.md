# CryptoChain

This is a simple Blockchain application which allow a node to join the network and create, sign, verify and broadcast transactions. 

Newly broadcasted transactions are then received by peer nodes in their local transaction pool and later verified and included in next block upon successful mining and the minor gets some reward for the same.

At a point of time the longest and valid blockchain is accepted by all the peer nodes. 

The mining difficulty is dynamically adjusted frequently to ensure timely block creation and mitigate 51% attack on the blockchain network. 

Elliptic Curve secp256k1 algorithm is used to securely sign and independently verify a transaction and Sha256 hashing algorithm for mining purpose.


This project was recognized by GitHub and shortlisted for GitHub Arctic Code Vault Archival program.
