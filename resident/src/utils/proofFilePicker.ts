import * as DocumentPicker from 'expo-document-picker';
import * as ImagePicker from 'expo-image-picker';

export type PickedFile = {
  uri: string;
  name: string;
  size: number | null;
  mimeType: string;
};

export const ALLOWED_PROOF_MIME_TYPES = ['image/jpeg', 'image/png', 'application/pdf'];
const MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024;

export const PROOF_FILE_ERROR_MESSAGE = 'File must be JPG, PNG, or PDF and not exceed 5 MB.';

// Client-side check only — the backend re-validates independently (PaymentProofService),
// since a client check can always be bypassed.
export function isValidProofFile(file: PickedFile): boolean {
  if (!ALLOWED_PROOF_MIME_TYPES.includes(file.mimeType)) {
    return false;
  }
  return file.size === null || file.size <= MAX_FILE_SIZE_BYTES;
}

function inferMimeType(uri: string, fallback: string): string {
  const extension = uri.split('.').pop()?.toLowerCase();
  if (extension === 'png') {
    return 'image/png';
  }
  if (extension === 'jpg' || extension === 'jpeg') {
    return 'image/jpeg';
  }
  if (extension === 'pdf') {
    return 'application/pdf';
  }
  return fallback;
}

export async function pickFromCamera(): Promise<PickedFile | null> {
  const permission = await ImagePicker.requestCameraPermissionsAsync();
  if (!permission.granted) {
    throw new Error('Camera access is needed to take a photo of your payment proof.');
  }

  const result = await ImagePicker.launchCameraAsync({ mediaTypes: 'images', quality: 0.8 });
  if (result.canceled || !result.assets[0]) {
    return null;
  }

  const asset = result.assets[0];
  return {
    uri: asset.uri,
    name: asset.fileName ?? `photo-${Date.now()}.jpg`,
    size: asset.fileSize ?? null,
    mimeType: asset.mimeType ?? inferMimeType(asset.uri, 'image/jpeg'),
  };
}

export async function pickFromLibrary(): Promise<PickedFile | null> {
  const permission = await ImagePicker.requestMediaLibraryPermissionsAsync();
  if (!permission.granted) {
    throw new Error('Photo library access is needed to attach an image.');
  }

  const result = await ImagePicker.launchImageLibraryAsync({ mediaTypes: 'images', quality: 0.8 });
  if (result.canceled || !result.assets[0]) {
    return null;
  }

  const asset = result.assets[0];
  return {
    uri: asset.uri,
    name: asset.fileName ?? `photo-${Date.now()}.jpg`,
    size: asset.fileSize ?? null,
    mimeType: asset.mimeType ?? inferMimeType(asset.uri, 'image/jpeg'),
  };
}

export async function pickFromFiles(): Promise<PickedFile | null> {
  const result = await DocumentPicker.getDocumentAsync({
    type: ALLOWED_PROOF_MIME_TYPES,
    copyToCacheDirectory: true,
  });
  if (result.canceled || !result.assets[0]) {
    return null;
  }

  const asset = result.assets[0];
  return {
    uri: asset.uri,
    name: asset.name,
    size: asset.size ?? null,
    mimeType: asset.mimeType ?? inferMimeType(asset.uri, 'application/octet-stream'),
  };
}
