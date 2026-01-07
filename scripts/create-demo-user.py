#!/usr/bin/env python3
"""
Demo User Creator for Melodee

This script creates a properly configured demo user with encrypted password
that matches Melodee's authentication expectations.

Usage:
    python3 create-demo-user.py [--connection-string CONNECTION_STRING]

Environment Variables:
    MELODEE_CONNECTION_STRING  PostgreSQL connection string
    MELODEE_ENCRYPTION_KEY     Encryption key (defaults to development key)
"""

import sys
import os
import base64
import hashlib
import uuid
import re
from datetime import datetime, timezone
from pathlib import Path

# Try to import required libraries
try:
    import psycopg2
    from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
    from cryptography.hazmat.backends import default_backend
    from cryptography.hazmat.primitives import padding
except ImportError as e:
    print("ERROR: Required Python packages not installed")
    print("Install with: pip3 install psycopg2-binary cryptography")
    print(f"Missing: {e}")
    sys.exit(1)


class MelodeeEncryption:
    """Encryption helper matching Melodee's C# EncryptionHelper"""
    
    @staticmethod
    def generate_public_key():
        """Generate a random base64-encoded public key (32 bytes)"""
        return base64.b64encode(os.urandom(32)).decode('utf-8')
    
    @staticmethod
    def encrypt(private_key: str, plain_text: str, public_key: str) -> str:
        """
        Encrypt plaintext using AES-256-CBC with PKCS7 padding.
        Matches the C# EncryptionHelper.Encrypt method.
        """
        try:
            # Derive key and IV from private_key and public_key
            # This matches the C# implementation which uses similar derivation
            key_material = (private_key + public_key).encode('utf-8')
            key = hashlib.sha256(key_material).digest()  # 32 bytes for AES-256
            iv = hashlib.md5(public_key.encode('utf-8')).digest()  # 16 bytes for IV
            
            # Pad plaintext to AES block size (128 bits = 16 bytes)
            padder = padding.PKCS7(128).padder()
            padded_data = padder.update(plain_text.encode('utf-8')) + padder.finalize()
            
            # Encrypt
            cipher = Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
            encryptor = cipher.encryptor()
            encrypted_data = encryptor.update(padded_data) + encryptor.finalize()
            
            # Return as base64
            return base64.b64encode(encrypted_data).decode('utf-8')
        except Exception as e:
            print(f"ERROR: Encryption failed: {e}")
            raise


def parse_connection_string(conn_str: str) -> dict:
    """Parse .NET-style connection string to Python dict"""
    params = {}
    for part in conn_str.split(';'):
        if '=' in part:
            key, value = part.split('=', 1)
            params[key.strip().lower()] = value.strip()
    
    return {
        'host': params.get('host', 'localhost'),
        'port': params.get('port', '5432'),
        'database': params.get('database', 'melodee'),
        'user': params.get('username', 'melodee'),
        'password': params.get('password', 'melodee')
    }


def create_demo_user(conn_params: dict, encryption_key: str):
    """Create the demo user with proper encryption"""
    
    print("╔════════════════════════════════════════════════════════════╗")
    print("║  Creating Demo User                                        ║")
    print("╚════════════════════════════════════════════════════════════╝")
    print()
    
    # Generate user credentials
    username = "demo"
    email = "demo@melodee.org"
    password = "Mel0deeR0cks!"
    
    # Generate encryption keys
    public_key = MelodeeEncryption.generate_public_key()
    api_key = str(uuid.uuid4())
    
    # Encrypt password using Melodee's encryption method
    print(f"Encrypting password with private key...")
    encrypted_password = MelodeeEncryption.encrypt(encryption_key, password, public_key)
    
    print(f"  Username: {username}")
    print(f"  Email: {email}")
    print(f"  Password: {password}")
    print(f"  API Key: {api_key}")
    print(f"  Public Key: {public_key[:20]}...")
    print(f"  Encrypted Password: {encrypted_password[:20]}...")
    print()
    
    # Connect to database
    try:
        conn = psycopg2.connect(**conn_params)
        cur = conn.cursor()
        
        # Check if demo user already exists
        cur.execute('SELECT "Id" FROM "Users" WHERE "UserNameNormalized" = %s', ('DEMO',))
        existing_user = cur.fetchone()
        
        if existing_user:
            print("Demo user already exists. Updating password...")
            
            # Update existing user
            cur.execute('''
                UPDATE "Users" 
                SET "PublicKey" = %s,
                    "PasswordEncrypted" = %s,
                    "LastUpdatedAt" = %s,
                    "Email" = %s,
                    "EmailNormalized" = %s
                WHERE "UserNameNormalized" = %s
            ''', (
                public_key,
                encrypted_password,
                datetime.now(timezone.utc),
                email,
                email.upper(),
                'DEMO'
            ))
        else:
            print("Creating new demo user...")
            
            # Insert new user
            cur.execute('''
                INSERT INTO "Users" (
                    "ApiKey",
                    "UserName",
                    "UserNameNormalized",
                    "Email",
                    "EmailNormalized",
                    "PublicKey",
                    "PasswordEncrypted",
                    "IsAdmin",
                    "IsEditor",
                    "HasSettingsRole",
                    "HasDownloadRole",
                    "HasUploadRole",
                    "HasPlaylistRole",
                    "HasCoverArtRole",
                    "HasCommentRole",
                    "HasPodcastRole",
                    "HasStreamRole",
                    "HasJukeboxRole",
                    "HasShareRole",
                    "IsScrobblingEnabled",
                    "TimeZoneId",
                    "CreatedAt",
                    "IsLocked"
                ) VALUES (
                    %s, %s, %s, %s, %s, %s, %s,
                    false, false, true, true, false, true,
                    true, true, true, true, true, true,
                    false, 'UTC', %s, false
                )
            ''', (
                api_key,
                username,
                username.upper(),
                email,
                email.upper(),
                public_key,
                encrypted_password,
                datetime.now(timezone.utc)
            ))
        
        conn.commit()
        cur.close()
        conn.close()
        
        print()
        print("✓ Demo user created successfully!")
        print()
        print("Demo credentials:")
        print("  Username: demo")
        print("  Password: Mel0deeR0cks!")
        print("  Email: demo@melodee.org")
        print()
        
        return True
        
    except Exception as e:
        print(f"ERROR: Failed to create demo user: {e}")
        return False


def main():
    # Get connection string
    conn_str = os.getenv('MELODEE_CONNECTION_STRING')
    if not conn_str:
        if '--connection-string' in sys.argv:
            idx = sys.argv.index('--connection-string')
            if idx + 1 < len(sys.argv):
                conn_str = sys.argv[idx + 1]
    
    if not conn_str:
        print("ERROR: PostgreSQL connection string not provided")
        print("Set MELODEE_CONNECTION_STRING environment variable or use --connection-string option")
        sys.exit(1)
    
    # Get encryption key
    encryption_key = os.getenv(
        'MELODEE_ENCRYPTION_KEY',
        'H+Kiik6VMKfTD2MesF1GoMjczTrD5RhuKckJ5+/UQWOdWajGcsEC3yEnlJ5eoy8Y'
    )
    
    # Parse connection string
    conn_params = parse_connection_string(conn_str)
    
    # Create demo user
    success = create_demo_user(conn_params, encryption_key)
    
    sys.exit(0 if success else 1)


if __name__ == '__main__':
    main()
