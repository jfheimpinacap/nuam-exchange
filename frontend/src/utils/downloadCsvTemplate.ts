const utf8Bom = '\uFEFF';

export function downloadCsvTemplate(fileName: string, content: string): void {
  const blob = new Blob([`${utf8Bom}${content}`], { type: 'text/csv;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');

  link.href = url;
  link.download = fileName;
  link.style.display = 'none';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}
