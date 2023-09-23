export type GalleryPhotoDto = {
  url: string;
  width: number;
  height: number;
};

export type GalleryPhotoListItem = GalleryPhotoDto & {
  id: number;
};
